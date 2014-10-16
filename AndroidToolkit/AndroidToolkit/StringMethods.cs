using System;
using System.Collections.Generic;
using System.Text;
using Action = AndroidDeviceConfig.Action;

namespace de.sebastianrutofski.AndroidToolkit
{
    public class StringMethods
    {
        public static string StringEnumToString(IEnumerable<string> enumeration, string separator = " ")
        {
            Action action;
            StringBuilder builder = new StringBuilder();

            foreach (string s in enumeration)
            {
                builder.Append(s + separator);
            }

            string concenatedstring = builder.Remove(builder.Length - (separator.Length + 1), separator.Length).ToString();
            return concenatedstring;
        }

        public static string GetStringBetween(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start, StringComparison.InvariantCulture) + start.Length;
            int endIndex = source.IndexOf(end, startIndex, StringComparison.InvariantCulture);
            int length = endIndex - startIndex;
            return source.Substring(startIndex, length);
        }
    }
}