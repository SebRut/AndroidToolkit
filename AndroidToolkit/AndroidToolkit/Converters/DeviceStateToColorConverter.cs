using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RegawMOD.Android;

namespace de.sebastianrutofski.AndroidToolkit.Converters
{
    public class DeviceStateToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DeviceState) value)
            {
                case DeviceState.ONLINE:
                    return Colors.Green;
                case DeviceState.UNKNOWN:
                    return Colors.Gray;
                case DeviceState.FASTBOOT:
                case DeviceState.RECOVERY:
                    return Colors.Orange;
                case DeviceState.OFFLINE:
                    return Colors.Red;
                default:
                    return Colors.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}