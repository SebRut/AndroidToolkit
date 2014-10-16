using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace de.sebastianrutofski.AndroidToolkit.Converters
{
    public class StringConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var stringBuilder = new StringBuilder();
            foreach (string value in values.OfType<string>())
            {
                stringBuilder.Append(value + " ");
            }
            return stringBuilder.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}