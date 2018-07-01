using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace VRChat_Stalker
{
    class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserStatus)
            {
                switch ((UserStatus)value)
                {
                    case UserStatus.Offline:
                        return Brushes.Gray;

                    case UserStatus.Online:
                        return Brushes.LightGreen;

                    case UserStatus.Private:
                        return Brushes.Yellow;
                }
            }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
