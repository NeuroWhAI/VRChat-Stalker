using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace VRChat_Stalker
{
    public class ProfileVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public ProfileVM()
        {
            CmdJoin = new CommandHandler((_) => Join());
            CmdRemoveTag = new CommandHandler((param) => RemoveTag(param as string));


#if DEBUG
            User = new VRCUser()
            {
                Id = "Test-ID",
                Name = "TEST NameLongLongHi",
                ImageUrl = @"http://www.personalbrandingblog.com/wp-content/uploads/2017/08/blank-profile-picture-973460_640-300x300.png",
                Star = 2,
                Location = "online",
                StatusText = "Status TEST Hi ZZfefefefefefefefFafaMamaThx",
                Permission = UserPermission.Trust | UserPermission.Avatar,
                Tags = new HashSet<string> { "test", "banana", "cookie", "서벌", "타노C", "파세파세호" }
            };
#endif
        }

        public ICommand CmdJoin { get; set; }
        public ICommand CmdRemoveTag { get; set; }

        public VRChatApi.VRChatApi Vrc { get; set; }
        private VRCUser m_user = null;
        public VRCUser User
        {
            get { return m_user; }
            set
            {
                m_user = value;

                UserTags = new ObservableCollection<string>(value.Tags);
                OnPropertyChanged("UserTags");

                OnPropertyChanged("User");
                OnPropertyChanged("CanJoin");
                OnPropertyChanged("JoinTooltip");
                OnPropertyChanged("IsTrusted");
                OnPropertyChanged("HasAvatarTag");
                OnPropertyChanged("HasWorldTag");
                OnPropertyChanged("HasLegendTag");
            }
        }

        public bool CanJoin => User.Status == UserStatus.Online;

        public string JoinTooltip
        {
            get
            {
                if (User.FriendsWith.Count <= 0)
                {
                    return User.InstanceId;
                }

                var buffer = new StringBuilder("With my friends: ");

                foreach (string friend in User.FriendsWith.Skip(1))
                {
                    buffer.Append(friend);
                    buffer.Append(", ");
                }

                buffer.Append(User.FriendsWith.Last());

                return buffer.ToString();
            }
        }

        public bool IsTrusted => (User.Permission & UserPermission.Trust) == UserPermission.Trust;
        public bool HasAvatarTag => (User.Permission & UserPermission.Avatar) == UserPermission.Avatar;
        public bool HasWorldTag => (User.Permission & UserPermission.World) == UserPermission.World;
        public bool HasLegendTag => (User.Permission & UserPermission.Legend) == UserPermission.Legend;

        public ObservableCollection<string> UserTags { get; set; } = new ObservableCollection<string>();

        private void Join()
        {
            string worldId = User.WorldId;
            string instId = User.InstanceId;

            string uri = $"https://vrchat.net/launch?worldId={worldId}&instanceId={instId}";

            System.Diagnostics.Process.Start(uri);
        }

        public bool AddTag(string tag)
        {
            if (tag.Length > 32)
            {
                return false;
            }

            tag = tag.ToLowerInvariant();

            bool result = User.Tags.Add(tag);

            if (result)
            {
                UserTags.Add(tag);
            }

            return result;
        }

        private void RemoveTag(string tag)
        {
            if (User.Tags.Contains(tag))
            {
                User.Tags.Remove(tag);

                UserTags.Remove(tag);
            }
        }
    }
}
