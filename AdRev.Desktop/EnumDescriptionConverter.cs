using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AdRev.Desktop
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var type = value.GetType();
            if (!type.IsEnum) return value.ToString() ?? string.Empty;

            var name = value.ToString();
            if (name == null) return string.Empty;

            var fieldInfo = type.GetField(name);
            if (fieldInfo == null) return name;

            var attributes = (DescriptionAttribute[]?)fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes != null && attributes.Length > 0) ? attributes[0].Description : name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
