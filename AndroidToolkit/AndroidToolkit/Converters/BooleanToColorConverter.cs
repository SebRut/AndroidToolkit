using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace de.sebastianrutofski.AndroidToolkit.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameters = (Color[]) parameter;
            return ((bool) value) ? parameters[0] : parameters[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}