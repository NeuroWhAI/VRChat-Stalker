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
    }
}
