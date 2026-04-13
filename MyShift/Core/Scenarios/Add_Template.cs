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
            if (!context.Data.ContainsKey("template"))
            {
                context.Data.Add("template", new ScheduleTemplate());
                await botClient.SendMessage(message.Chat, "Выберите тип графика", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Линейны", "0"), new InlineKeyboardButton("Циклический", "1")), cancellationToken: ct);
                context.CurrentStep = "SetScheduleType";
                return ScenarioResult.Transition;
            }
            else if (context.CurrentStep == "SetScheduleType")
            {
                ((ScheduleTemplate)context.Data["template"]).Type = Int32.Parse(context.Data["Callback"].ToString());
                context.Data.Add("typeTemplate", Int32.Parse(context.Data["Callback"].ToString()));
                context.CurrentStep = null;
            }
            if (((ScheduleTemplate)context.Data["template"]).Type == 0)
            {
                switch (context.CurrentStep)
                {
                    case null:
                        ((ScheduleTemplate)context.Data["template"]).CreatorBy = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(long.Parse(context.Data["TelegramUserId"].ToString()), ct)).Id, ct);
                        await botClient.SendMessage(message.Chat, "Введите название шаблона", cancellationToken: ct);
                        context.CurrentStep = "selectDayWeek";
                        return ScenarioResult.Transition;
                    case "selectDayWeek":
                        ((ScheduleTemplate)context.Data["template"]).Name = message.Text;
                        await botClient.SendMessage(message.Chat, "Введите список дней недели в цифрах, где Пн -> 1, Вт -> 2...Вс ->7. Пример ввода: 1,3,5,7", cancellationToken: ct);
                        context.CurrentStep = "selectDayPart";
                        return ScenarioResult.Transition;
                    case "selectDayPart":
                        TextIsValidate(0, message.Text);
                        DayTemplate dayTemplate = new DayTemplate();
                        dayTemplate.Days = string.Join(',', message.Text.Split(',').Select(Int32.Parse).OrderBy(n => n));
                        context.Data["templateJson"] = dayTemplate;
                        await botClient.SendMessage(message.Chat, "День или ночь?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("День", "TypeShift"), new InlineKeyboardButton("Ночь", "Night")), cancellationToken: ct);
                        context.CurrentStep = "selectedStartTime";
                        return ScenarioResult.Transition;
                    case "selectedStartTime":
                        ((DayTemplate)context.Data["templateJson"]).Type = context.Data["Callback"].ToString();
                        await botClient.SendMessage(message.Chat, "Введите время начала смены в формате '05:25'", cancellationToken: ct);
                        context.CurrentStep = "selectedEndTime";
                        return ScenarioResult.Transition;
                    case "selectedEndTime":
                        TextIsValidate(1, message.Text.Trim());
                        ((DayTemplate)context.Data["templateJson"]).Start = TimeSpan.Parse(message.Text);
                        context.CurrentStep = "createTemplate";
                        await botClient.SendMessage(message.Chat, "Введите время окончания смены в формате '05:25'", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    case "createTemplate":
                        TextIsValidate(1, message.Text);
                        ((DayTemplate)context.Data["templateJson"]).End = TimeSpan.Parse(message.Text);
                        break;
                }
            }
            else
            {
                bool restart = true;
                while (restart)
                {
                    restart = false;
                    switch (context.CurrentStep)
                    {
                        case null:
                            ((ScheduleTemplate)context.Data["template"]).CreatorBy = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(long.Parse(context.Data["TelegramUserId"].ToString()), ct)).Id, ct);
                            await botClient.SendMessage(message.Chat, "Введите название шаблона", cancellationToken: ct);
                            context.CurrentStep = "StartCycl";
                            return ScenarioResult.Transition;
                        case "StartCycl":
                            ((ScheduleTemplate)context.Data["template"]).Name = message.Text;
                            await botClient.SendMessage(message.Chat, "Создать первую смену?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Да", "yes"), new InlineKeyboardButton("Нет", "no")), cancellationToken: ct);
                            context.CurrentStep = "SelectDayPart";
                            return ScenarioResult.Transition;
                        case "SelectDayPart":
                            if (context.Data["Callback"].ToString() == "no")
                            {
                                break;
                            }
                            if (!context.Data.ContainsKey("templateJson"))
                                context.Data.Add("templateJson", new CycleTemplate());
                            await botClient.EditMessageText(context.Data["ChatId"].ToString(), message.MessageId, "Выберите тип смены", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Дневная", "Day"), new InlineKeyboardButton("Ночная", "Night"), new InlineKeyboardButton("Выходной", "off")), cancellationToken: ct);
                            context.CurrentStep = "SelectedStartTime";
                            return ScenarioResult.Transition;
                        case "SelectedStartTime":
                            CycleItem cycleItem = new CycleItem();
                            cycleItem.TypeShift = context.Data["Callback"].ToString();
                            ((CycleTemplate)context.Data["templateJson"]).Add(cycleItem);
                            if(context.Data["Callback"].ToString() == "off")
                            {
                                restart = true;
                                context.CurrentStep = "CreateNew";
                                continue;
                            }
                            await botClient.SendMessage(message.Chat, "Введите время начала смены в формате '05:25'", cancellationToken: ct);
                            context.CurrentStep = "SelectedEndTime";
                            return ScenarioResult.Transition;
                        case "SelectedEndTime":
                            TextIsValidate(1, message.Text);
                            ((CycleTemplate)context.Data["templateJson"]).LastOrDefault().Start = TimeSpan.Parse(message.Text);
                            await botClient.SendMessage(message.Chat, "Введите время окончания смены в формате '05:25'", cancellationToken: ct);
                            context.CurrentStep = "CreateNew";
                            return ScenarioResult.Transition;
                        case "CreateNew":
                            if (context.Data["Callback"].ToString() != "off")
                            {
                                TextIsValidate(1, message.Text.Trim());
                                ((CycleTemplate)context.Data["templateJson"]).LastOrDefault().End = TimeSpan.Parse(message.Text);
                            }
                            await botClient.SendMessage(message.Chat, "Создать следующую смену?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Да", "yes"), new InlineKeyboardButton("Нет", "no")), cancellationToken: ct);
                            context.CurrentStep = "SelectDayPart";
                            return ScenarioResult.Transition;
                    }
                }
            }
            ScheduleTemplate scheduleTemplate = ((ScheduleTemplate)context.Data["template"]);
            if (!context.Data.ContainsKey("templateJson"))
            {
                await botClient.SendMessage(message.Chat, "Шаблон не создан т.к. не добавленно ни одной смены.", cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            if (context.Data["typeTemplate"] != null && context.Data["typeTemplate"].ToString() == "0")
            {
                scheduleTemplate.RulesJson = JsonSerializer.Serialize((DayTemplate)context.Data["templateJson"]);
            }
            else if(context.Data["typeTemplate"] != null && context.Data["typeTemplate"].ToString() == "1")
            {
                scheduleTemplate.RulesJson = JsonSerializer.Serialize((CycleTemplate)context.Data["templateJson"]);
            }
            else
            {
                await botClient.SendMessage(message.Chat, "Шаблон не может быть пустым!", cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            await _scheduleRequestService.InsertScheduleTemplateAsync(scheduleTemplate, ct);
            await botClient.SendMessage(message.Chat, "Шаблон успешно добавлен!", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
        private void TextIsValidate(int check, string text)
        {
            if (check == 0 && (!Regex.IsMatch(text, @"(^\d{1}$)|(^(\d,)+\d{1}$)") || (text.Contains("8") || text.Contains("9") || text.Contains("0"))))
            {
                throw new FormatException("Значение не должно быть пустым и должно быть в формате: 1,3,5,6,7");
            }
            else if (check == 1 && (!Regex.IsMatch(text, @"^([01]\d|2[0-3]):[0-5]\d$")))
            {
                throw new FormatException("Время должно быть в формате от 00:00 до 23:59");
            }
        }
    }
}