using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChat_Stalker
{
    public enum UserStatus
    {
        Online,
        Private,
        Offline,
    }

    [Flags]
    public enum UserPermission
    {
        None = 0,
        Trust = 1,
        Avatar = 1 << 1,
        World = 1 << 2,
        Legend = 1 << 3,
    }

    public class VRCUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Star { get; set; } = 1;
        public bool IsTracked { get; set; } = false;
        public string ImageUrl { get; set; }
        public string StatusText { get; set; }
        public string Memo { get; set; } = "";
        public List<string> FriendsWith { get; set; } = new List<string>();
        public UserPermission Permission { get; set; } = UserPermission.None;
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
    }
}
