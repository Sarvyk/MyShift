using MyShift.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Helpers
{
    internal static class EnumBitConverter
    {//времянка т.к. вероятно придётся думать насчё универсальности.
        public static string GetFromBitToShortNames(string str)
        {
            int bit = Convert.ToInt32(str, 2);
            Weekday weekdays = (Weekday)bit;
            string result = string.Empty;
            foreach (Weekday weekday in Enum.GetValues(typeof(Weekday)))
            {
                if (weekdays.HasFlag(weekday))
                    result += $"{weekday.GetDisplayShortName()},";
            }
            return result.Remove(result.Length - 1);
        }
        public static string GetFromEnumToBit(string enumStr)
        {
            //тут происходит образование строки через флаги по ShortName атрибуту.
            Weekday result = Weekday.none;
            Weekday[] weekDays = Enum.GetValues(typeof(Weekday)).Cast<Weekday>().Where(week => week != Weekday.none).ToArray();
            string[] weekDaysMass = enumStr.Split(",");
            for (int i = 0; i < weekDays.Length; i++)
            {
                if (enumStr.Contains(weekDays[i].GetDisplayShortName().ToLower()))
                    result |= weekDays[i];
            }
            return Convert.ToString((int)result, 2).PadLeft(7, '0');
        }
        public static Weekday GetEnumFromBit(string enumBit)
        {
            int bitDecim = Convert.ToInt32(enumBit, 2);
            Weekday weekdays = (Weekday)bitDecim;
            return weekdays;
        }
        public static HashSet<string> GetEnumFromBitToMass(string enumBit)
        {
            Weekday flags = GetEnumFromBit(enumBit);
            Weekday[] enumMass = Enum.GetValues(typeof(Weekday)).Cast<Weekday>().Where(week => week != Weekday.none).ToArray();
            HashSet<string> result = new HashSet<string>();
            for (int i =0; i<enumMass.Length;i++)
            {
                if (flags.HasFlag(enumMass[i]))
                {
                    result.Add(enumMass[i].GetDisplayShortName().ToLower());
                }
            }
            return result;
        }
    }
}