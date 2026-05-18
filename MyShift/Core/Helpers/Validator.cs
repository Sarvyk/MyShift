using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShift.Core.Helpers
{
    internal static class Validator
    {
        public static void TextIsValidate(int check, string text)
        {
            switch (check)
            {
                case 0:
                    var numbers = text.Split(',');
                    if ((!Regex.IsMatch(text, @"(^\d{1}$)|(^(\d,)+\d{1}$)") || (text.Contains("8") || text.Contains("9") || text.Contains("0"))) || numbers.Length != numbers.Distinct().Count())
                    {
                        throw new FormatException("Значение не должно быть пустым, не должно быть дублей и должно быть в формате: 1,3,5,6,7⚠️");
                    }
                    break;
                case 1:
                    if (!Regex.IsMatch(text, @"^([01]\d|2[0-3]):[0-5]\d$"))
                    {
                        throw new FormatException("Время должно быть в формате от 00:00 до 23:59⚠️");
                    }
                    break;
            }
        }
    }
}