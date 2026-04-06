using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Services;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Dialogs
{
    internal class DialogCreateTemplate : Dialog<CreateScheduleTemplateStage>
    {
        private readonly ScheduleBuilder _builder;
        public DialogCreateTemplate(ITelegramBotClient botClient, Update update, ScheduleBuilder scheduleBuilder, IScheduleRequestService schReqService) : base(botClient, update, schReqService)
        {
            _builder = scheduleBuilder;
        }

        public override async Task<bool> NextStep(string? message, CancellationToken ct)
        {
            Validate(message);
            switch (_stage)
            {
                case CreateScheduleTemplateStage.Name:
                    _builder.AddNameTemplate(message);
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите время начала рабочего дня в формате \"08:00\"", cancellationToken: ct);
                    return false;
                case CreateScheduleTemplateStage.TimeBeg:
                    _builder.AddStartTimeTemplate(TimeSpan.Parse(message));
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите время окончания рабочего дня в формате \"08:00\"", cancellationToken: ct);
                    return false;
                case CreateScheduleTemplateStage.TimeEnd:
                    _builder.AddEndTimeTemplate(TimeSpan.Parse(message));
                    _stage++;
                    await _botClient.SendMessage(_update.Message.Chat, $"Укажите дни недели в формате \"Пн,Вт,Ср,Чт,Пт,Сб,Вс\"", cancellationToken: ct);
                    return false;
                case CreateScheduleTemplateStage.Weekday:
                    _builder.AddDaysOfWeekTemplate(EnumBitConverter.GetFromEnumToBit(message.ToLower()));
                    await _scheduleRequestService.InsertScheduleTemplateAsync(_builder.GetTemplate(), ct);
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
                case CreateScheduleTemplateStage.TimeBeg:
                    if (!Regex.IsMatch(str,@"^[0-9]{1,2}:[0-9]{2}$") || !TimeSpan.TryParse(str, out resultBeg) || resultBeg.TotalHours>24 || resultBeg.TotalHours<0)
                    {
                        throw new FormatException("Не правильный ответ. Пример правильного ввода:\"8:15\"");
                    }
                    break;
                case CreateScheduleTemplateStage.TimeEnd:
                    if (!Regex.IsMatch(str, @"^[0-9]{1,2}:[0-9]{2}$") || !TimeSpan.TryParse(str, out resultBeg) || resultBeg.TotalHours > 24 || resultBeg.TotalHours < 0)
                    {
                        throw new FormatException("Не правильный ответ. Пример правильного ввода:\"8:15\"");
                    }
                    break;
                case CreateScheduleTemplateStage.Weekday:
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
    }
    internal enum CreateScheduleTemplateStage
    {
        Name,
        TimeBeg,
        TimeEnd,
        Weekday
    }
}