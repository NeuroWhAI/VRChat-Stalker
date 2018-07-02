using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media;
using System.Net;

namespace VRChat_Stalker
{
    public class UserChangedEventArgs
    {
        public string ImageUrl { get; set; }
        public string UserName { get; set; }
        public string UserStatus { get; set; }
    }

    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public MainVM()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);


#if DEBUG
            // TEST
            Users.Add(new VRCUser()
            {
                Id = "test-user",
                Location = "offline",
                StatusText = "LongtextLongtextLongte",
                ImageUrl = "http://www.personalbrandingblog.com/wp-content/uploads/2017/08/blank-profile-picture-973460_640-300x300.png",
                Name = "TEST User 바나나사과파인애플",
            });
            Users.Add(new VRCUser()
            {
                Id = "test-user2",
                Location = "offline",
                StatusText = "Offline",
                ImageUrl = "http://www.personalbrandingblog.com/wp-content/uploads/2017/08/blank-profile-picture-973460_640-300x300.png",
                Name = "TEST User",
            });
#endif


            if (File.Exists("alarm.mp3"))
            {
                m_mediaAlarm = new MediaPlayer();
                m_mediaAlarm.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "alarm.mp3")));

                UserChanged += (_) =>
                  {
                      m_mediaAlarm.Dispatcher.Invoke(() =>
                      {
                          m_mediaAlarm.Stop();
                          m_mediaAlarm.Play();
                      });
                  };
            }


            InitUserListView();
        }
        
        public VRChatApi.VRChatApi Vrc { get; set; }
        private WorldCache m_worldCache = new WorldCache();

        public ObservableCollection<VRCUser> Users { get; set; } = new ObservableCollection<VRCUser>();
        public ListCollectionView UserListView { get; set; }
        private SortDescription m_sortDesc = new SortDescription()
        {
            PropertyName = "Star",
            Direction = ListSortDirection.Descending
        };

        private DispatcherTimer m_checkTimer = new DispatcherTimer();

        public event Action<UserChangedEventArgs> UserChanged;

        private MediaPlayer m_mediaAlarm = null;

        public void InitUserListView()
        {
            UserListView = (ListCollectionView)CollectionViewSource.GetDefaultView(Users);
            UserListView.GroupDescriptions.Add(new PropertyGroupDescription { PropertyName = "Status" });
            UserListView.SortDescriptions.Add(new SortDescription { PropertyName = "Status" });
            UserListView.SortDescriptions.Add(m_sortDesc);

            OnPropertyChanged("UserListView");
        }

        public async void Init()
        {
            Users = await GetAllFriends();

            InitUserListView();

            LoadUsers();


            m_checkTimer.Stop();
            m_checkTimer = new DispatcherTimer();
            m_checkTimer.Interval = TimeSpan.FromSeconds(8);
            m_checkTimer.Tick += CheckTimer_Tick;
            m_checkTimer.Start();
        }

        public void Close()
        {
            m_checkTimer.Stop();

            SaveUsers();
        }

        private Version GetLatestVersion()
        {
            try
            {
                var client = new WebClient();
                string info = client.DownloadString(new Uri(@"https://raw.githubusercontent.com/NeuroWhAI/VRChat-Stalker/release/VRChat%20Stalker/Properties/AssemblyInfo.cs"));
                
                int index = info.LastIndexOf("AssemblyVersion");
                index = info.IndexOf('\"', index + 1);

                int endIndex = info.IndexOf('\"', index + 1);

                if (endIndex > index)
                {
                    return Version.Parse(info.Substring(index + 1, endIndex - index - 1));
                }
            }
            catch (WebException)
            {
                return null;
            }


            return null;
        }

        public bool CheckUpdate()
        {
            var latestVersion = GetLatestVersion();

            if (latestVersion == null)
            {
                return false;
            }

            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            return latestVersion > thisVersion;
        }

        private void SaveUsers()
        {
            try
            {
                using (var bw = new BinaryWriter(new FileStream("_users.dat", FileMode.Create)))
                {
                    bw.Write(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

                    bw.Write(Users.Count);

                    foreach (var user in Users)
                    {
                        bw.Write(user.Id);
                        bw.Write(user.Star);
                        bw.Write(user.IsTracked);
                    }


                    bw.Close();
                }

                File.Copy("_users.dat", "users.dat", true);
                File.Delete("_users.dat");
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        private void LoadUsers()
        {
            if (File.Exists("users.dat") == false)
            {
                return;
            }

            using (var br = new BinaryReader(new FileStream("users.dat", FileMode.Open)))
            {
                Version fileVersion = Version.Parse(br.ReadString());

                int usrCnt = br.ReadInt32();

                for (int i = 0; i < usrCnt; ++i)
                {
                    string id = br.ReadString();
                    int star = br.ReadInt32();
                    bool isTracked = br.ReadBoolean();

                    foreach (var user in Users)
                    {
                        if (user.Id == id)
                        {
                            user.Star = star;
                            user.IsTracked = isTracked;

                            break;
                        }
                    }
                }


                br.Close();
            }


            UserListView.Refresh();
        }

        private async void CheckTimer_Tick(object sender, EventArgs e)
        {
            m_checkTimer.Stop();


            var onlineUsers = await GetFriends(false);

            foreach (var user in Users)
            {
                if (user.Location != "offline" && onlineUsers.Any(u => u.Id == user.Id) == false)
                {
                    user.Location = "offline";
                    user.StatusText = "Offline";

                    // Alarm offline.
                    if (user.IsTracked)
                    {
                        UserChanged?.Invoke(user.ImageUrl, user.Name, user.StatusText);
                    }
                }
            }

            foreach (var user in onlineUsers)
            {
                VRCUser target = null;

                foreach (var u in Users)
                {
                    if (user.Id == u.Id)
                    {
                        target = u;
                        break;
                    }
                }

                if (target == null)
                {
                    Users.Add(user);

                    // Alarm online.
                    if (user.IsTracked)
                    {
                        UserChanged?.Invoke(user.ImageUrl, user.Name, $"Online ({user.StatusText})");
                    }
                }
                else
                {
                    if (target.IsTracked)
                    {
                        if (target.Status != user.Status)
                        {
                            // Alarm status changed.
                            UserChanged?.Invoke(user.ImageUrl, user.Name, user.StatusText);
                        }
                        else if (target.Star >= 2)
                        {
                            if (target.Location != user.Location)
                            {
                                // Alarm location changed.
                                UserChanged?.Invoke(user.ImageUrl, user.Name, user.StatusText);
                            }
                        }

                        if (target.Star >= 3)
                        {
                            if (target.Name != user.Name)
                            {
                                // Alarm name changed.
                                UserChanged?.Invoke(user.ImageUrl, user.Name, $"{target.Name} → {user.Name}");
                            }

                            if (target.ImageUrl != user.ImageUrl)
                            {
                                // Alarm image changed.
                                UserChanged?.Invoke(user.ImageUrl, user.Name, "Avatar changed");
                            }
                        }
                    }

                    target.Name = user.Name;
                    target.Location = user.Location;
                    target.ImageUrl = user.ImageUrl;
                    target.StatusText = user.StatusText;
                }
            }

            
            UserListView.Refresh();


            SaveUsers();


            m_checkTimer.Start();
        }

        public void FilterUsers(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                UserListView.Filter = (obj) =>
                {
                    return true;
                };
            }
            else
            {
                filter = filter.ToLowerInvariant();

                UserListView.Filter = (obj) =>
                {
                    if (obj is VRCUser user
                    && (user.Name.ToLowerInvariant().Contains(filter)
                    || user.StatusText.ToLowerInvariant().Contains(filter)))
                    {
                        return true;
                    }

                    return false;
                };
            }
        }

        public void SortUsers(SortTypes by)
        {
            UserListView.SortDescriptions.Remove(m_sortDesc);

            if (by == SortTypes.Name)
            {
                m_sortDesc.PropertyName = "Name";
                m_sortDesc.Direction = ListSortDirection.Ascending;
            }
            else if (by == SortTypes.Status)
            {
                m_sortDesc.PropertyName = "Status";
                m_sortDesc.Direction = ListSortDirection.Ascending;
            }
            else if (by == SortTypes.Star)
            {
                m_sortDesc.PropertyName = "Star";
                m_sortDesc.Direction = ListSortDirection.Descending;
            }

            UserListView.SortDescriptions.Add(m_sortDesc);
        }

        private async Task<string> ConvertLocation(string location, string userId)
        {
            if (location == "offline")
            {
                return "Offline";
            }
            else if (location == "private")
            {
                return "Private";
            }
            else if (location.Contains(':'))
            {
                var loc = location.Split(':');

                if (loc[1] == userId)
                {
                    return "Private";
                }
                else
                {
                    string worldId = loc[0];
                    string instanceId = loc[1];

                    var world = await m_worldCache.GetWorld(Vrc, worldId);

                    if (world != null)
                    {
                        var instance = await m_worldCache.GetInstance(Vrc, worldId, instanceId);

                        if (instance != null && instance.users != null)
                        {
                            return string.Format("{0} ({1}/{2})", world.name,
                                instance.users.Count, world.capacity);
                        }
                        else
                        {
                            return world.name;
                        }
                    }
                }
            }

            return location;
        }

        public async Task<List<VRCUser>> GetFriends(bool isOffline)
        {
            var users = new List<VRCUser>();


            int offset = 0;
            int retry = 3;

            while (true)
            {
                var friends = await Vrc.FriendsApi.Get(offset, 20, isOffline);

                if (friends == null)
                {
                    if (retry > 0)
                    {
                        await Task.Delay(1000);

                        retry -= 1;

                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var res in friends)
                {
                    var user = new VRCUser()
                    {
                        Id = res.id,
                        Name = res.displayName,
                        Location = res.location,
                        ImageUrl = res.currentAvatarThumbnailImageUrl,
                    };

                    user.StatusText = await ConvertLocation(res.location, res.id);

                    users.Add(user);
                }

                if (friends.Count < 20)
                {
                    break;
                }

                offset += friends.Count;
            }


            return users;
        }

        public async Task<ObservableCollection<VRCUser>> GetAllFriends()
        {
            var users = new ObservableCollection<VRCUser>();


            var onlines = await GetFriends(false);
            var offlines = await GetFriends(true);

            foreach (var user in onlines)
            {
                users.Add(user);
            }

            foreach (var user in offlines)
            {
                users.Add(user);
            }


            return users;
        }
    }
}
