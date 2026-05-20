using Microsoft.EntityFrameworkCore;
using MyShift.Core.Entities;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Helpers;
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
            if (!context.Data.ContainsKey("templateType"))
            {
                switch (context.CurrentStep)
                {
                    case null:
                        await botClient.SendMessage(message.Chat, "Процесс создания шаблона", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                        context.Data.Add("currentMessage", await botClient.SendMessage(message.Chat, "Введите название шаблона✍️", cancellationToken: ct));
                        context.CurrentStep = "SetScheduleName";
                        return ScenarioResult.Transition;
                    case "SetScheduleName":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                        context.Data.Add("templateName", message.Text);
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите тип графика🗓️", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Линейный➡️", "0"), new InlineKeyboardButton("Циклический🔁", "1")), cancellationToken: ct);
                        context.CurrentStep = "SetScheduleType";
                        return ScenarioResult.Transition;
                    case "SetScheduleType":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                        context.Data.Add("templateType", context.Data["Callback"].ToString());
                        context.Data.Remove("Callback");
                        context.CurrentStep = null;
                        break;
                }
            }
            if (context.Data["templateType"].ToString() == "0")
            {
                return await ProccessScenarioFirstVariantTemplate(botClient, context, message, ct);
            }
            else
            {
                return await ProccessScenarioSecondVariantTemplate(botClient, context, message, ct);
            }
        }
        /// <summary>
        /// Создаёт шаблон линейного графика
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ProccessScenarioFirstVariantTemplate(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите список дней недели в цифрах, где Пн -> 1, Вт -> 2...Вс ->7. Пример ввода: 1,3,5,7✍️🔢", cancellationToken: ct);
                    context.CurrentStep = "selectDayPart";
                    return ScenarioResult.Transition;
                case "selectDayPart":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                    Validator.TextIsValidate(0, message.Text);
                    DayTemplate dayTemplate = new DayTemplate();
                    dayTemplate.Days = string.Join(',', message.Text.Split(',').Select(Int32.Parse).OrderBy(n => n));
                    context.Data.Add("templateJson", dayTemplate);
                    context.Data["currentMessage"]= await botClient.SendMessage(message.Chat, "День☀️ или ночь🌙?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton(ShiftType.day.GetDisplayShortName(), "1"), new InlineKeyboardButton(ShiftType.night.GetDisplayShortName(), "2")), cancellationToken: ct);
                    context.CurrentStep = "selectedStartTime";
                    return ScenarioResult.Transition;
                case "selectedStartTime":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                    ((DayTemplate)context.Data["templateJson"]).Type = (ShiftType)Int32.Parse(context.Data["Callback"].ToString());
                    await botClient.SendMessage(message.Chat, "Введите время🕔 начала смены в формате '05:25'", cancellationToken: ct);
                    context.CurrentStep = "selectedEndTime";
                    return ScenarioResult.Transition;
                case "selectedEndTime":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                    Validator.TextIsValidate(1, message.Text.Trim());
                    ((DayTemplate)context.Data["templateJson"]).Start = TimeSpan.Parse(message.Text);
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите время🕔 окончания смены в формате '05:25'", cancellationToken: ct);
                    context.CurrentStep = "createTemplate";
                    return ScenarioResult.Transition;
                case "createTemplate":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                    Validator.TextIsValidate(1, message.Text);
                    ((DayTemplate)context.Data["templateJson"]).End = TimeSpan.Parse(message.Text);
                    break;
            }
            return await InsertNewTemplate(botClient, context, message, ct);
        }
        /// <summary>
        /// Создаёт шаблон цикличного графика
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> ProccessScenarioSecondVariantTemplate(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            bool restart = true;
            while (restart)
            {
                restart = false;
                switch (context.CurrentStep)
                {
                    case null:
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Создать первую смену❓", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("Да✅", "yes"), new InlineKeyboardButton("Нет❌", "no")), cancellationToken: ct);
                        context.CurrentStep = "SelectDayPart";
                        return ScenarioResult.Transition;
                    case "SelectDayPart":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                        if (context.Data["Callback"].ToString() == "no")
                        {
                            await botClient.EditMessageText(message.Chat, message.MessageId, "Выбор отменён", cancellationToken: ct);
                            break;
                        }
                        if (!context.Data.ContainsKey("templateJson"))
                            context.Data.Add("templateJson", new CycleTemplate());
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите тип смены🌓", replyMarkup: 
                            new InlineKeyboardMarkup(
                                new InlineKeyboardButton(ShiftType.day.GetDisplayShortName() + "☀️", "1"), 
                                new InlineKeyboardButton(ShiftType.night.GetDisplayShortName() + "🌙", "2"), 
                                new InlineKeyboardButton(ShiftType.off.GetDisplayShortName() + "🛌", "3")), cancellationToken: ct);
                        context.CurrentStep = "SelectedStartTime";
                        return ScenarioResult.Transition;
                    case "SelectedStartTime":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                        CycleItem cycleItem = new CycleItem();
                        cycleItem.TypeShift = (ShiftType)Int32.Parse(context.Data["Callback"].ToString());
                        ((CycleTemplate)context.Data["templateJson"]).Add(cycleItem);
                        if (context.Data["Callback"].ToString() == "3")
                        {
                            context.Data["currentMessage"] = null;
                            restart = true;
                            await botClient.EditMessageText(message.Chat, message.MessageId, "Добавлен выходной день", cancellationToken: ct);
                            context.CurrentStep = "CreateNew";
                            continue;
                        }
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите время🕔 начала смены в формате '05:25'", cancellationToken: ct);
                        context.CurrentStep = "SelectedEndTime";
                        return ScenarioResult.Transition;
                    case "SelectedEndTime":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                        Validator.TextIsValidate(1, message.Text);
                        ((CycleTemplate)context.Data["templateJson"]).LastOrDefault().Start = TimeSpan.Parse(message.Text);
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите время🕔 окончания смены в формате '05:25'", cancellationToken: ct);
                        context.CurrentStep = "CreateNew";
                        return ScenarioResult.Transition;
                    case "CreateNew":
                        Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                        if (context.Data["Callback"].ToString() != "3")
                        {
                            Validator.TextIsValidate(1, message.Text.Trim());
                            ((CycleTemplate)context.Data["templateJson"]).LastOrDefault().End = TimeSpan.Parse(message.Text);
                        }
                        context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Создать следующую смену❓", replyMarkup: 
                            new InlineKeyboardMarkup(
                                new InlineKeyboardButton("Да✅", "yes"), 
                                new InlineKeyboardButton("Нет❌", "no")), cancellationToken: ct);
                        context.CurrentStep = "SelectDayPart";
                        return ScenarioResult.Transition;
                }
            }
            return await InsertNewTemplate(botClient, context, message, ct);
        }
        /// <summary>
        /// Добавляет созданный шаблон в список шаблонов
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<ScenarioResult> InsertNewTemplate(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            ToDoUser creator = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            if (!context.Data.ContainsKey("templateJson"))
            {
                await botClient.SendMessage(message.Chat, "Шаблон не создан т.к. не добавленно ни одной смены.📅❌", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(creator.Role), cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            ScheduleTemplate scheduleTemplate = new ScheduleTemplate()
            {
                Type = Int32.Parse(context.Data["templateType"].ToString()),
                Name = context.Data["templateName"].ToString(),
                CreatorId = creator.Id,
            };
            if (context.Data["templateType"].ToString() == "0")
            {
                scheduleTemplate.RulesJson = JsonSerializer.Serialize((DayTemplate)context.Data["templateJson"]);
            }
            else if (context.Data["templateType"].ToString() == "1")
            {
                scheduleTemplate.RulesJson = JsonSerializer.Serialize((CycleTemplate)context.Data["templateJson"]);
            }
            await _scheduleRequestService.InsertScheduleTemplateAsync(scheduleTemplate, ct);
            await botClient.SendMessage(message.Chat, "Шаблон успешно добавлен!✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(creator.Role), cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}