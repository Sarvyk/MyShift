using MyShift.Core.Entities;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Scenarios
{
    internal class Add_Template : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Add_Template(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Add_Template;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["creatorId"] = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(message.From.Id, ct)).Id,ct);
                    await botClient.SendMessage(message.Chat, "Введите название шаблона", cancellationToken: ct);
                    context.CurrentStep = "SelectTemplateType";
                    return ScenarioResult.Transition;
                case "SelectTemplateType":
                    context.Data["name"] = message.Text;
                    await botClient.SendMessage(message.Chat, "Выберите тип графика", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Линейны", "0"), new InlineKeyboardButton("Циклический", "1")), cancellationToken: ct);
                    context.CurrentStep = "SelectDayes";
                    return ScenarioResult.Transition;
                case "SelectDayes":
                    if (context.Data["Callback"].ToString() == "0")
                    {//в таком варианте будет создаваться шаблон линейного графика
                        context.Data["typeTemplate"] = 0;
                        await botClient.SendMessage(message.Chat, "Введите список дней в цифрах, где Пн -> 1, Вт -> 2...Вс ->7. Пример ввода: 1,3,5,7", cancellationToken: ct);
                        context.CurrentStep = "SelectDayPart";
                        return ScenarioResult.Transition;
                    }
                    else
                    {
                        return ScenarioResult.Transition;
                    }
                case "SelectDayPart":
                    if (context.Data["typeTemplate"].ToString() == "0")
                    {
                        Validate(0, message.Text);
                        context.Data["days"] = message.Text;
                        await botClient.SendMessage(message.Chat, "День, ночь или выходной?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("День", "Day"), new InlineKeyboardButton("Ночь", "Night")), cancellationToken: ct);
                        context.CurrentStep = "SelectedStartTime";
                        return ScenarioResult.Transition;
                    }
                    else
                    {
                        return ScenarioResult.Completed;
                    }
                case "SelectedStartTime":
                    if (context.Data["typeTemplate"].ToString() == "0")
                    {
                        context.Data["type"] = context.Data["Callback"].ToString();
                        await botClient.SendMessage(message.Chat, "Введите время начала смены в формате '05:25'", cancellationToken: ct);
                        context.CurrentStep = "SelectedEndTime";
                        return ScenarioResult.Transition;
                    }
                    else
                        return ScenarioResult.Transition;
                case "SelectedEndTime":
                    if (context.Data["typeTemplate"].ToString() == "0")
                    {
                        Validate(1, message.Text.Trim());
                        context.Data["start"] = message.Text;
                        await botClient.SendMessage(message.Chat, "Введите время окончания смены в формате '05:25'", cancellationToken: ct);
                        context.CurrentStep = "CreateTemplate";
                        return ScenarioResult.Transition;
                    }
                    else
                        return ScenarioResult.Transition;
                case "CreateTemplate":
                    if (context.Data["typeTemplate"].ToString() == "0")
                    {
                        Validate(1, message.Text);
                        context.Data["end"] = message.Text;
                        break;
                    }
                    else
                        break;
            }
            ScheduleTemplate scheduleTemplate = null;
            if (context.Data["typeTemplate"].ToString() == "0")
            {
                DaySchedule daySchedule = new DaySchedule()
                {
                    Type = context.Data["type"].ToString(),
                    Start = TimeSpan.Parse(context.Data["start"].ToString()),
                    End = TimeSpan.Parse(context.Data["end"].ToString())
                };
                scheduleTemplate = new ScheduleTemplate()
                {
                    Name = context.Data["name"].ToString(),
                    Type = Int32.Parse(context.Data["typeTemplate"].ToString()),
                    RulesJson = JsonSerializer.Serialize(daySchedule),
                    CreatorBy = (ToDoUser)context.Data["creatorId"]
                };
            }
            else
            {

            }
            await _scheduleRequestService.InsertScheduleTemplateAsync(scheduleTemplate, ct);
            await botClient.SendMessage(message.Chat, "Шаблон успешно добавлен!", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
        private void Validate(int check, string text)
        {
            if(check == 0 && (!Regex.IsMatch(text, @"(^\d{1}$)|(^(\d,)+\d{1}$)") || (text.Contains("8") || text.Contains("9"))))
            {
                throw new ArgumentException("Значение не должно быть пустым и должно быть в формате: 1,3,5,6,7");
            }
            else if(check == 1 && (!Regex.IsMatch(text, @"^([01]\d|2[0-3]):[0-5]\d$")))
            {
                throw new ArgumentException("Время должно быть в формате от 00:00 до 23:59");
            }
        }
    }
}