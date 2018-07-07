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
                IsTracked = true,
                Id = "test-user",
                Location = "Korea:12345~private(123)~test(456)",
                StatusText = "LongtextLongtextLongtextLongText",
                ImageUrl = "http://www.personalbrandingblog.com/wp-content/uploads/2017/08/blank-profile-picture-973460_640-300x300.png",
                Name = "TEST User 바나나사과파인애플",
                InstanceOccupant = "(7/20)",
            });
            Users.Add(new VRCUser()
            {
                IsTracked = true,
                Id = "test-useruser",
                Location = "Korea:12345~hidden(123)~nonce(456)",
                StatusText = "Korea",
                ImageUrl = "http://www.personalbrandingblog.com/wp-content/uploads/2017/08/blank-profile-picture-973460_640-300x300.png",
                Name = "TEST User123",
                InstanceOccupant = "(0/20)",
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
                      if (Option.PlaySound)
                      {
                          m_mediaAlarm.Dispatcher.Invoke(() =>
                          {
                              m_mediaAlarm.Stop();
                              m_mediaAlarm.Play();
                          });
                      }
                  };
            }


            InitUserListView();
        }
        
        public VRChatApi.VRChatApi Vrc { get; set; }
        private WorldCache m_worldCache = new WorldCache();

        private bool m_onLoading = false;
        public bool OnLoading
        {
            get { return m_onLoading; }
            set
            {
                m_onLoading = value;
                OnPropertyChanged("OnLoading");
            }
        }

        public ProgramOption Option { get; set; } = new ProgramOption();

        public ObservableCollection<VRCUser> Users { get; set; } = new ObservableCollection<VRCUser>();
        public ListCollectionView UserListView { get; set; }
        private SortDescription m_sortDesc = new SortDescription()
        {
            PropertyName = "Star",
            Direction = ListSortDirection.Descending
        };

        private Dictionary<string, int> m_userIdToIndex = new Dictionary<string, int>();

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
            OnLoading = true;

            Option.Load();

            Users = await GetAllFriends();

            foreach (int i in Enumerable.Range(0, Users.Count))
            {
                m_userIdToIndex[Users[i].Id] = i;
            }

            InitUserListView();

            LoadUsers();

            OnLoading = false;


            m_checkTimer.Stop();
            m_checkTimer = new DispatcherTimer();
            m_checkTimer.Interval = TimeSpan.FromSeconds(Option.UpdateCycle);
            m_checkTimer.Tick += CheckTimer_Tick;
            m_checkTimer.Start();
        }

        public void UpdateOption()
        {
            m_checkTimer.Interval = TimeSpan.FromSeconds(Option.UpdateCycle);
        }

        public void Close()
        {
            if (m_checkTimer.IsEnabled)
            {
                m_checkTimer.Stop();

                SaveUsers().Wait();
            }
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

        public Task SaveUsers()
        {
            return Task.Factory.StartNew(()=>
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
                            bw.Write(user.Memo);

                            bw.Write(user.Tags.Count);
                            foreach (string tag in user.Tags)
                            {
                                bw.Write(tag);
                            }
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
            });
        }

        private void LoadUsers()
        {
            if (File.Exists("users.dat") == false)
            {
                return;
            }

            try
            {
                Version memoVersion = new Version(1, 0, 4, 0);
                Version tagVersion = new Version(1, 0, 5, 0);

                using (var br = new BinaryReader(new FileStream("users.dat", FileMode.Open)))
                {
                    Version fileVersion = Version.Parse(br.ReadString());

                    int usrCnt = br.ReadInt32();

                    for (int i = 0; i < usrCnt; ++i)
                    {
                        string id = br.ReadString();
                        int star = br.ReadInt32();
                        bool isTracked = br.ReadBoolean();

                        string memo = "";
                        if (fileVersion >= memoVersion)
                        {
                            memo = br.ReadString();
                        }

                        HashSet<string> tags = new HashSet<string>();
                        if (fileVersion >= tagVersion)
                        {
                            int count = br.ReadInt32();

                            for (int t = 0; t < count; ++t)
                            {
                                tags.Add(br.ReadString());
                            }
                        }

                        if (m_userIdToIndex.ContainsKey(id))
                        {
                            var user = Users[m_userIdToIndex[id]];

                            user.Star = star;
                            user.IsTracked = isTracked;
                            user.Memo = memo;
                            user.Tags = tags;
                        }
                    }


                    br.Close();
                }
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }


            UserListView.Refresh();
        }

        private void OnUserChanged(string imageUrl, string userName, string userStatus)
        {
            UserChanged?.Invoke(new UserChangedEventArgs()
            {
                ImageUrl = imageUrl,
                UserName = userName,
                UserStatus = userStatus,
            });
        }

        private async void CheckTimer_Tick(object sender, EventArgs e)
        {
            m_checkTimer.Stop();


            var onlineUsers = await GetFriends(false);

            var onlineIndices = new List<int>();

            foreach (var user in onlineUsers)
            {
                if (m_userIdToIndex.ContainsKey(user.Id))
                {
                    onlineIndices.Add(m_userIdToIndex[user.Id]);
                }
            }

            onlineIndices.Sort();


            int checkIndex = 0;

            // Check offline.
            for (int i = 0; i < Users.Count; ++i)
            {
                if (checkIndex < onlineIndices.Count
                    && onlineIndices[checkIndex] == i)
                {
                    ++checkIndex;

                    continue;
                }


                var user = Users[i];

                if (user.Location != "offline")
                {
                    user.Location = "offline";
                    user.StatusText = "Offline";
                    user.FriendsWith.Clear();

                    // Refresh
                    Users.RemoveAt(i);
                    Users.Insert(i, user);

                    // Alarm offline.
                    if (user.IsTracked)
                    {
                        OnUserChanged(user.ImageUrl, user.Name, user.StatusText);
                    }
                }
            }

            foreach (var user in onlineUsers)
            {
                VRCUser target = null;
                int targetIndex = -1;

                if (m_userIdToIndex.ContainsKey(user.Id))
                {
                    targetIndex = m_userIdToIndex[user.Id];
                    target = Users[targetIndex];
                }

                if (target == null)
                {
                    m_userIdToIndex[user.Id] = Users.Count;

                    Users.Add(user);

                    // Alarm online.
                    if (user.IsTracked)
                    {
                        OnUserChanged(user.ImageUrl, user.Name, $"Online ({user.StatusText})");
                    }
                }
                else
                {
                    if (target.IsTracked)
                    {
                        if (target.Name != user.Name)
                        {
                            // Alarm name changed.
                            OnUserChanged(user.ImageUrl, user.Name, $"{target.Name} → {user.Name}");
                        }

                        if (target.Status != user.Status)
                        {
                            // Alarm status changed.
                            OnUserChanged(user.ImageUrl, user.Name, user.StatusText);
                        }
                        else if (target.Star >= 2)
                        {
                            if (target.Location != user.Location)
                            {
                                // Alarm location changed.
                                if (string.IsNullOrWhiteSpace(user.InstanceNumber))
                                {
                                    OnUserChanged(user.ImageUrl, user.Name, user.StatusText);
                                }
                                else
                                {
                                    OnUserChanged(user.ImageUrl, user.Name,
                                        string.Format("{0} {1}", user.StatusText, user.InstanceNumber));
                                }
                            }
                        }

                        if (target.Star >= 3)
                        {
                            if (target.ImageUrl != user.ImageUrl)
                            {
                                // Alarm image changed.
                                OnUserChanged(user.ImageUrl, user.Name, "Avatar changed");
                            }

                            if (target.Permission != user.Permission)
                            {
                                // Alarm permission changed.
                                OnUserChanged(user.ImageUrl, user.Name, "Permission changed");
                            }


                            var newFriends = new List<string>();

                            foreach (string friend in user.FriendsWith)
                            {
                                if (!target.FriendsWith.Contains(friend))
                                {
                                    newFriends.Add(friend);
                                }
                            }

                            if (newFriends.Count > 0)
                            {
                                var buffer = new StringBuilder("Now with ");

                                foreach (string friend in newFriends.Skip(1))
                                {
                                    buffer.Append(friend);
                                    buffer.Append(", ");
                                }

                                buffer.Append(newFriends.First());

                                // Alarm playmate changed.
                                OnUserChanged(user.ImageUrl, user.Name, buffer.ToString());
                            }
                        }
                    }


                    bool changed = false;

                    if (target.Name != user.Name)
                    {
                        target.Name = user.Name;
                        changed = true;
                    }
                    if (target.Location != user.Location)
                    {
                        target.Location = user.Location;
                        changed = true;
                    }
                    if (target.ImageUrl != user.ImageUrl)
                    {
                        target.ImageUrl = user.ImageUrl;
                        changed = true;
                    }
                    if (target.StatusText != user.StatusText)
                    {
                        target.StatusText = user.StatusText;
                        changed = true;
                    }
                    if (target.InstanceOccupant != user.InstanceOccupant)
                    {
                        target.InstanceOccupant = user.InstanceOccupant;
                        changed = true;
                    }

                    target.FriendsWith = user.FriendsWith;
                    target.Permission = user.Permission;


                    if (changed)
                    {
                        // Refresh
                        Users.RemoveAt(targetIndex);
                        Users.Insert(targetIndex, target);
                    }
                }
            }


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
                string lower = filter.ToLowerInvariant();

                UserListView.Filter = (obj) =>
                {
                    if (obj is VRCUser user
                    && (user.Name.ToLowerInvariant().Contains(lower)
                    || user.StatusText.ToLowerInvariant().Contains(lower)
                    || user.Tags.Contains(filter)))
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

        private async Task ConvertLocation(VRCUser user)
        {
            string location = user.Location;
            string userId = user.Id;

            user.InstanceOccupant = "";

            if (location == "offline")
            {
                user.StatusText = "Offline";
            }
            else if (location == "private")
            {
                user.StatusText = "Private";
            }
            else if (location.Contains(':'))
            {
                var loc = location.Split(':');

                if (loc[1] == userId)
                {
                    user.StatusText = "Private";
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
                            user.StatusText = world.name;
                            user.InstanceOccupant = string.Format("({0}/{1})", instance.users.Count,
                                world.capacity);

                            // 같은 인스턴스에 있으면서 나랑도 친구인 사람을 목록화.
                            user.FriendsWith = instance.users
                                .Where(u => m_userIdToIndex.ContainsKey(u.id) && u.id != userId)
                                .Select(u => u.displayName)
                                .ToList();
                        }
                        else
                        {
                            user.StatusText = world.name;
                        }
                    }
                }
            }
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

                    foreach (string tag in res.tags)
                    {
                        if (tag.Contains("legend"))
                        {
                            user.Permission |= UserPermission.Legend;
                        }
                        else if (tag.Contains("avatar"))
                        {
                            user.Permission |= UserPermission.Avatar;
                        }
                        else if (tag.Contains("world"))
                        {
                            user.Permission |= UserPermission.World;
                        }
                        else if (tag.Contains("trust"))
                        {
                            user.Permission |= UserPermission.Trust;
                        }
                    }

                    await ConvertLocation(user);

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
