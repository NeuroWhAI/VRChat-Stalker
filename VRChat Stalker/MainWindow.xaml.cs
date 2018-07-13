using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VRChat_Stalker
{
    using NotifyIcon = System.Windows.Forms.NotifyIcon;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(VRChatApi.VRChatApi vrc)
        {
            InitializeComponent();


            Vm = (this.DataContext as MainVM);
            Vm.Vrc = vrc;
            Vm.UserChanged += Vm_UserChanged;

            trayIcon.Visible = true;
            trayIcon.Icon = Properties.Resources.icon;
            trayIcon.Text = this.Title;
            trayIcon.DoubleClick += new EventHandler((sender, arg) => this.Show());
        }

        private MainVM Vm { get; set; }

        private NotifyIcon trayIcon = new NotifyIcon();

        private bool m_willExit = false;

        private async Task HideAndSave()
        {
            this.Hide();

            await Vm.SaveUsers();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Vm.Init();


            // Check update
            if (Vm.CheckUpdate())
            {
                ShowDownloadAlarm();
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Visibility == Visibility.Visible && m_willExit == false)
            {
                // Hide

                e.Cancel = true;

                await HideAndSave();
            }
            else
            {
                // Exit

                trayIcon.Visible = false;

                Vm.Close();
            }
        }

        private async void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            string tag = (string)item.Tag;


            // Close side menu.
            this.btnToggleMenu.IsChecked = false;


            if (tag == "Logout")
            {
                // Logout
                m_willExit = true;

                var loginWin = new LoginWindow();
                loginWin.AutoLogin = false;
                Application.Current.MainWindow = loginWin;

                this.Close();

                loginWin.Show();
            }
            else if (tag == "Option")
            {
                // Option
                var optionWin = new OptionWindow(Vm.Option);
                optionWin.Owner = this;
                optionWin.ShowDialog();
                optionWin.Close();

                Vm.UpdateOption();
            }
            else if (tag == "Update")
            {
                // Check update
                if (Vm.CheckUpdate())
                {
                    ShowDownloadAlarm();
                }
                else
                {
                    this.snackBar.MessageQueue.Enqueue("No update required!");
                }
            }
            else if (tag == "Hide")
            {
                // Hide
                await this.HideAndSave();
            }
            else if (tag == "Exit")
            {
                // Exit
                m_willExit = true;
                this.Close();
                Application.Current.Shutdown();
            }
        }

        private void ShowDownloadAlarm()
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.snackBar.MessageQueue.Enqueue("New version detected!", "DOWNLOAD", () =>
                {
                    System.Diagnostics.Process.Start(@"http://neurowhai.tistory.com/202");
                });
            }
            else
            {
                this.trayIcon.ShowBalloonTip(5000, "VRChat Stalker", "New version detected!",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private void Button_Sort_Name_Click(object sender, RoutedEventArgs e)
        {
            Vm.SortUsers(SortTypes.Name);
        }

        private void Button_Sort_Location_Click(object sender, RoutedEventArgs e)
        {
            Vm.SortUsers(SortTypes.Status);
        }

        private void Button_Sort_Star_Click(object sender, RoutedEventArgs e)
        {
            Vm.SortUsers(SortTypes.Star);
        }

        private void Button_Show_All_Click(object sender, RoutedEventArgs e)
        {
            Vm.FilterUsers(this.boxSearch.Text, FilterTypes.All);
        }

        private void Button_Hide_Offline_Click(object sender, RoutedEventArgs e)
        {
            Vm.FilterUsers(this.boxSearch.Text, FilterTypes.HideOffline);
        }

        private void Button_Hide_Untracked_Click(object sender, RoutedEventArgs e)
        {
            Vm.FilterUsers(this.boxSearch.Text, FilterTypes.HideUntracked);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Vm.FilterUsers(this.boxSearch.Text);
        }

        private void Vm_UserChanged(UserChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.snackBar.MessageQueue.Enqueue(string.Format("{0} - {1}", e.UserName, e.UserStatus));
            }

            this.trayIcon.ShowBalloonTip(5000, e.UserName, e.UserStatus,
                System.Windows.Forms.ToolTipIcon.Info);
        }

        private void listUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.listUser.UnselectAll();
        }

        private async void MenuItem_Details_Click(object sender, RoutedEventArgs e)
        {
            if (this.listUser.SelectedIndex < 0)
            {
                return;
            }


            var user = this.listUser.SelectedItem as VRCUser;

            if (user == null)
            {
                return;
            }

            var profileWin = new ProfileWindow(Vm.Vrc, user);
            profileWin.Owner = this;
            profileWin.ShowDialog();
            profileWin.Close();

            
            await Vm.SaveUsers();
        }

        private async void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (Equals(eventArgs.Parameter, true)
                && this.listUser.SelectedIndex >= 0
                && this.listUser.SelectedItem is VRCUser user)
            {
                bool success = await Vm.DeleteFriend(user);

                if (success)
                {
                    this.snackBar.MessageQueue.Enqueue("Successfully deleted!");
                }
                else
                {
                    this.snackBar.MessageQueue.Enqueue("Fail to unfriend!");
                }
            }
        }
    }
}
