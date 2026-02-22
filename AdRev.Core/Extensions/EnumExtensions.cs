<<<<<<< HEAD
                                                                                                                                    using System;
=======
using System;
>>>>>>> origin/main
using System.ComponentModel;
using System.Reflection;

namespace AdRev.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo? fi = value.GetType().GetField(value.ToString());

            if (fi == null) return value.ToString();

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
