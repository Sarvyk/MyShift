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
            FieldInfo? info = value?.GetType().GetField(value.ToString());
            DisplayAttribute? datr = info?.GetCustomAttribute<DisplayAttribute>();
            return datr?.GetName() ?? "";
        }
    }
}
