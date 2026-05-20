using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Scenarios.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Core.Helpers
{
    internal static class Validator
    {
        /// <summary>
        /// Проверка правильности ввода времени и дней недели
        /// </summary>
        /// <param name="check"></param>
        /// <param name="text"></param>
        /// <exception cref="FormatException"></exception>
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
        /// <summary>
        /// Проверяет правильность ответа. Если требуется текстовый ввод, то другие варианты вызовут ошибку. В варианте с inline кнопками так же.
        /// </summary>
        /// <param name="prevMessage"></param>
        /// <param name="usedMessage"></param>
        /// <param name="expectedResponse"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ValidateCurrentMessage(Message? prevMessage, Message usedMessage, int expectedResponse)
        {
            if (prevMessage == null)
                return;
            if(expectedResponse == 0 && prevMessage.MessageId != usedMessage.MessageId)
            {
                throw new ArgumentException("Ожидался выбор из последнего сообщения сценария!");
            }
            else if(expectedResponse == 1 && usedMessage.ReplyMarkup != null)
            {
                throw new ArgumentException("Ожидался текст");
            }
        }
    }
}