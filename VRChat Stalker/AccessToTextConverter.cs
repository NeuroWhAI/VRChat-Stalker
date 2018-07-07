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
    class AccessToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorldTags)
            {
                switch ((WorldTags)value)
                {
                    case WorldTags.FriendsPlus:
                        return "Friends+";

                    case WorldTags.InvitePlus:
                        return "Invite+";

                    case WorldTags.None:
                        return "";

                    default:
                        return ((WorldTags)value).ToString();
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
