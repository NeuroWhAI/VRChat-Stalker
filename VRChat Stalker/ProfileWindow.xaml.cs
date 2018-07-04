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
    /// <summary>
    /// ProfileWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(VRChatApi.VRChatApi vrc, VRCUser user)
        {
            InitializeComponent();


            Vm = (this.DataContext as ProfileVM);
            Vm.Vrc = vrc;
            Vm.User = user;
        }

        private ProfileVM Vm { get; set; }

        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (Equals(eventArgs.Parameter, true)
                && !string.IsNullOrWhiteSpace(boxTagName.Text))
            {
                bool success = Vm.AddTag(boxTagName.Text.Trim());

                if (!success)
                {
                    this.snackBar.MessageQueue.Enqueue("Invalid tag!");
                }
            }


            this.boxTagName.Text = "";
        }
    }
}
