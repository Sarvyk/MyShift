using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Helpers
{
    public static class EnumExt
    {
        public static string GetDisplayName(this Enum value)
        {
            DisplayAttribute? datr = GetDisplayAttribute(value);
            return datr?.GetName() ?? "";
        }
        public static string GetDisplayShortName(this Enum value)
        {
            DisplayAttribute? datr = GetDisplayAttribute(value);
            return datr?.GetShortName() ?? "";
        }
        private static DisplayAttribute? GetDisplayAttribute(Enum value)
        {
            FieldInfo? info = value?.GetType().GetField(value.ToString());
            return info?.GetCustomAttribute<DisplayAttribute>();
        }
    }
}
