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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Vm.Init();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;


            Vm.Close();
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == this.listMainMenu.Items[0])
            {
                // Logout
                var loginWin = new LoginWindow();
                loginWin.AutoLogin = false;
                Application.Current.MainWindow = loginWin;
                this.Close();
                loginWin.Show();
            }
            else if(sender == this.listMainMenu.Items[1])
            {
                // Option

            }
            else if (sender == this.listMainMenu.Items[2])
            {
                // Hide
                this.Hide();
            }
            else if (sender == this.listMainMenu.Items[3])
            {
                // Exit
                this.Close();
                Application.Current.Shutdown();
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Vm.FilterUsers(this.boxSearch.Text);
        }

        private void Vm_UserChanged(string imageUrl, string name, string status)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.snackBar.MessageQueue.Enqueue(string.Format("{0} - {1}", name, status));
            }

            this.trayIcon.ShowBalloonTip(5000, name, status,
                System.Windows.Forms.ToolTipIcon.Info);
        }
    }
}
