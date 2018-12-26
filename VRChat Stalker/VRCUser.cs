using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRChat_Stalker
{
    public enum UserStatus
    {
        Online,
        Private,
        Offline,
    }

    [Flags]
    public enum UserTags
    {
        None = 0,
        Trust = 1,
        Avatar = 1 << 1,
        World = 1 << 2,
        Legend = 1 << 3,
        Troll = 1 << 4,
    }

    public enum WorldTags
    {
        None,
        Public,
        FriendsPlus,
        Friends,
        InvitePlus,
        Invite,
    }

    public class VRCUser : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // 최신 정보를 담은 newData의 데이터를 적용.
        // 네트워크에서 지속적으로 받아오는 정보가 아니면 적용하지 않음.
        public void Update(VRCUser newData)
        {
            Name = newData.Name;
            Location = newData.Location;
            ImageUrl = newData.ImageUrl;
            StatusText = newData.StatusText;
            InstanceOccupant = newData.InstanceOccupant;
            StatusDescription = newData.StatusDescription;
            FriendsWith = newData.FriendsWith.ToList(); // Deep copy
            Permission = newData.Permission;
        }

        public string Id { get; set; }

        private string m_name;
        public string Name
        {
            get => m_name;
            set
            {
                if(value != m_name)
                {
                    m_name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_location;
        public string Location
        {
            get => m_location;
            set
            {
                if (value != m_location)
                {
                    m_location = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Status");
                    NotifyPropertyChanged("WorldId");
                    NotifyPropertyChanged("InstanceId");
                    NotifyPropertyChanged("InstanceNumber");
                    NotifyPropertyChanged("InstanceAccess");
                }
            }
        }

        private int m_star = 1;
        public int Star
        {
            get => m_star;
            set
            {
                if (value != m_star)
                {
                    m_star = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool m_isTracked = false;
        public bool IsTracked
        {
            get => m_isTracked;
            set
            {
                if (value != m_isTracked)
                {
                    m_isTracked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_imageUrl;
        public string ImageUrl
        {
            get => m_imageUrl;
            set
            {
                if (value != m_imageUrl)
                {
                    m_imageUrl = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_statusText;
        public string StatusText
        {
            get => m_statusText;
            set
            {
                if (value != m_statusText)
                {
                    m_statusText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_instanceOccupant;
        public string InstanceOccupant
        {
            get => m_instanceOccupant;
            set
            {
                if (value != m_instanceOccupant)
                {
                    m_instanceOccupant = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string m_memo = "";
        public string Memo
        {
            get => m_memo;
            set
            {
                if (value != m_memo)
                {
                    m_memo = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ProfileTooltip");
                }
            }
        }

        private string m_statusDescription;
        public string StatusDescription
        {
            get => m_statusDescription;
            set
            {
                if (value != m_statusDescription)
                {
                    m_statusDescription = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("ProfileTooltip");
                    NotifyPropertyChanged("StatusTooltip");
                }
            }
        }


        public List<string> FriendsWith { get; set; } = new List<string>();
        public UserTags Permission { get; set; } = UserTags.None;
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        public UserStatus Status
        {
            get
            {
                if (Location == "offline")
                {
                    return UserStatus.Offline;
                }
                else if (Location == "private")
                {
                    return UserStatus.Private;
                }
                else
                {
                    var loc = Location.Split(':');

                    if (loc.Length >= 2 && loc[1] == Id)
                    {
                        return UserStatus.Private;
                    }
                }

                return UserStatus.Online;
            }
        }

        public string WorldId
        {
            get
            {
                if (Location.Contains(':'))
                {
                    return Location.Split(':')[0];
                }

                return string.Empty;
            }
        }

        public string InstanceId
        {
            get
            {
                if (Location.Contains(':'))
                {
                    return Location.Split(':')[1];
                }

                return string.Empty;
            }
        }

        public string InstanceNumber
        {
            get
            {
                if (Location.Contains(':'))
                {
                    return "#" + Location.Split(':')[1].Split('~')[0];
                }

                return string.Empty;
            }
        }

        public WorldTags InstanceAccess
        {
            get
            {
                string id = InstanceId;

                if(string.IsNullOrWhiteSpace(id))
                {
                    return WorldTags.None;
                }

                if (id.Contains('~'))
                {
                    if (id.Contains("hidden"))
                    {
                        return WorldTags.FriendsPlus;
                    }
                    else if (id.Contains("friends"))
                    {
                        return WorldTags.Friends;
                    }
                    else if (id.Contains("can"))
                    {
                        return WorldTags.InvitePlus;
                    }
                    else if (id.Contains("private"))
                    {
                        return WorldTags.Invite;
                    }
                }

                return WorldTags.Public;
            }
        }

        /// <summary>
        /// 친구 목록 화면에서의 상태 정보
        /// </summary>
        public string ProfileTooltip
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Memo))
                {
                    if (string.IsNullOrWhiteSpace(this.StatusDescription))
                    {
                        return this.Id;
                    }

                    return this.StatusDescription;
                }

                return this.Memo;
            }
        }

        /// <summary>
        /// 프로필 화면에서의 상태 정보
        /// </summary>
        public string StatusTooltip
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.StatusDescription))
                {
                    return this.Id;
                }

                return this.StatusDescription;
            }
        }
    }
}
