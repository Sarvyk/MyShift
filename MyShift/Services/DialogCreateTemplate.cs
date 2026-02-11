using MyShift.Abstracts;
using MyShift.Enums;
using MyShift.Helpers;
using MyShift.Models;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Services
{
    internal class DialogCreateTemplate : Dialog
    {
        private Stage _stage;
        private ScheduleBuilder _scheduleBuilder;
        public DialogCreateTemplate(ITelegramBotClient botClient, Update update, ScheduleBuilder scheduleBuilder) : base(botClient, update)
        {
            _scheduleBuilder = scheduleBuilder;
        }

        public override async Task<bool> NextStep(string? message, CancellationToken ct)
        {
            Validate(message);
            switch (_stage)
            {
                case Stage.Name:
                    _scheduleBuilder.AddName(message);
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите время начала рабочего дня в формате \"08:00\"", cancellationToken: ct);
                    return false;
                case Stage.TimeBeg:
                    _scheduleBuilder.AddStartTime(TimeSpan.Parse(message));
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите время окончания рабочего дня в формате \"08:00\"", cancellationToken: ct);
                    return false;
                case Stage.TimeEnd:
                    _scheduleBuilder.AddEndTime(TimeSpan.Parse(message));
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите дни недели в формате \"Пн,Вт,Ср,Чт,Пт,Сб,Вс\"", cancellationToken: ct);
                    return false;
                case Stage.Weekday:
                    _scheduleBuilder.AddDaysOfWeek(((int)GetBitStr(message.ToLower())).ToString());
                    await _scheduleBuilder.AddToDataBase(ct);
                    await _botClient.SendMessage(_update.Message.Chat, $"Шаблон добавлен!", cancellationToken: ct);
                    break;
            }
            return true;
        }
        protected override void Validate(string? str)
        {
            base.Validate(str);
            TimeSpan resultBeg = new TimeSpan();
            switch (_stage)
            {//проверяем по маске ввод. TryParse сделан, чтобы сделать ещё проверку на часы.
                case Stage.TimeBeg:
                    if (!Regex.IsMatch(str,@"^[0-9]{1,2}:[0-9]{2}$") || !TimeSpan.TryParse(str, out resultBeg) || resultBeg.TotalHours>24 || resultBeg.TotalHours<0)
                    {
                        throw new FormatException("Не правильный ответ. Пример правильного ввода:\"8:15\"");
                    }
                    break;
                case Stage.TimeEnd:
                    if (!Regex.IsMatch(str, @"^[0-9]{1,2}:[0-9]{2}$") || !TimeSpan.TryParse(str, out resultBeg) || resultBeg.TotalHours > 24 || resultBeg.TotalHours < 0)
                    {
                        throw new FormatException("Не правильный ответ. Пример правильного ввода:\"8:15\"");
                    }
                    break;
                case Stage.Weekday:
                    string[] weekDays = str.Split(',');
                    if (weekDays.Length > 7)
                        throw new FormatException("Дней недели не может быть больше 7");
                    for (int i = 0; i < weekDays.Length; i++)
                    {
                        bool find = false;
                        Weekday[] weekDaysMass = Enum.GetValues(typeof(Weekday)).Cast<Weekday>().ToArray();
                        for (int j = 0; j < weekDaysMass.Length; j++)
                        {
                            string displayEnumName = weekDaysMass[j].GetDisplayShortName().ToLower();
                            if (displayEnumName == weekDays[i].Trim().ToLower())
                            {
                                find = true;
                            }
                        }
                        if (!find)
                            throw new FormatException("Не правильный ответ. Пример правильного ввода:\"Пн,Вт,Ср,Чт,Пт,Сб,Вс\"");
                    }
                    break;
            }
        }
        private Weekday GetBitStr(string weekDayStr)
        {//тут происходит образование строки через флаги по ShortName атрибуту.
            Weekday result = Weekday.none;
            Weekday[] weekDays = Enum.GetValues(typeof(Weekday)).Cast<Weekday>().Where(week => week != Weekday.none).ToArray();
            string[] weekDaysMass = weekDayStr.Split(",");
            for (int i = 0;i < weekDays.Length; i++)
            {
                if (weekDayStr.Contains(weekDays[i].GetDisplayShortName().ToLower()))
                    result |= weekDays[i];
            }
            return result;
        }
        private enum Stage
        {
            Name,
            TimeBeg,
            TimeEnd,
            Weekday
        }
    }
}