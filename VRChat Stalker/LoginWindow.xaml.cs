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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VRChat_Stalker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public bool AutoLogin { get; set; } = true;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoLogin)
            {
                LoginAuto();
            }
        }

        private async void LoginAuto()
        {
            this.barLogin.Visibility = Visibility.Visible;


            var vm = (this.DataContext as LoginVM);

            if (vm.AutoLogin)
            {
                var vrc = await vm.Login();
                if (vrc != null)
                {
                    GotoMainWindow(vrc);
                }
                else
                {
                    this.txtError.Visibility = Visibility.Visible;
                }
            }


            this.barLogin.Visibility = Visibility.Hidden;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.barLogin.Visibility == Visibility.Visible)
            {
                return;
            }

            this.barLogin.Visibility = Visibility.Visible;
            this.txtError.Visibility = Visibility.Hidden;


            var vm = (this.DataContext as LoginVM);

            var vrc = await vm.Login(this.boxId.Text, this.boxPwd.Password);

            if (vrc != null)
            {
                GotoMainWindow(vrc);
            }
            else
            {
                this.boxPwd.Password = "";

                this.txtError.Visibility = Visibility.Visible;
            }


            this.barLogin.Visibility = Visibility.Hidden;
        }

        private void GotoMainWindow(VRChatApi.VRChatApi vrc)
        {
            var mainWin = new MainWindow(vrc);
            Application.Current.MainWindow = mainWin;
            this.Close();
            mainWin.Show();
        }
    }
}
