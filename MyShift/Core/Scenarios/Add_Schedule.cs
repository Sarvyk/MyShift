using Microsoft.EntityFrameworkCore;
using MyShift.Core.Enums;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Scenarios
{
    internal class Add_Schedule : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Add_Schedule(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Add_Schedule;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            ScenarioResult result = ScenarioResult.Transition;
            switch (context.CurrentStep)
            {
                case null:
                    return await ShowTemplate(botClient, context, message, ct);
                case "NextPage":
                    return await NextPageWithSelectTemplate(botClient, context, message, ct);
                case "SelectUser":
                    result = await NextPageWithSelectUser(botClient, context, message, ct);
                    break;
            }
            return result;
        }
        private async Task<ScenarioResult> ShowTemplate(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            string callback = context.Data["Callback"].ToString();
            IReadOnlyList<ScheduleTemplate> templates = await _scheduleRequestService.GetAllTemplatesAsync(ct);
            if (templates.Count == 0)
            {
                await botClient.SendMessage(message.Chat, "Действующие шаблоны не найдены!🔍❌", cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            var callbackData = new List<KeyValuePair<string, string>>();
            await botClient.SendMessage(message.Chat, "Процесс создания шаблона📋", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
            foreach (ScheduleTemplate template in templates)
            {
                callbackData.Add(new KeyValuePair<string, string>(template.Name, ToDoItemCallbackDto.FromString($"selectTemplate|{template.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Список шаблонов📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showTemplatePageNext||0")), cancellationToken: ct);
            context.CurrentStep = "NextPage";
            return ScenarioResult.Transition;
        }
        private async Task<ScenarioResult> NextPageWithSelectTemplate(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            string callback = context.Data["Callback"].ToString();
            if (callback.StartsWith("selectTemplate|"))
            {
                context.Data.Add("TemplateId", ToDoItemCallbackDto.FromString(callback).ToDoItemId);
                return await ShowUser(botClient, context, message, ct);
            }
            IReadOnlyList<ScheduleTemplate> templates = await _scheduleRequestService.GetAllTemplatesAsync(ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (ScheduleTemplate template in templates)
            {
                callbackData.Add(new KeyValuePair<string, string>(template.Name, ToDoItemCallbackDto.FromString($"selectTemplate|{template.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите шаблон", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString($"{callback}")), cancellationToken: ct);
            context.CurrentStep = "NextPage";
            return ScenarioResult.Transition;
        }
        private async Task<ScenarioResult> ShowUser(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            string callback = context.Data["Callback"].ToString();
            if (callback.StartsWith("selectedUser|"))
            {
                int templateId = Int32.Parse(context.Data["TemplateId"].ToString());
                int userId = ToDoItemCallbackDto.FromString(callback).ToDoItemId;
                return await CreateScheduleForUser(botClient, context, message, templateId, userId, ct);
            }
            IReadOnlyList<ToDoUser> users = await _userService.GetAllUsers(ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (ToDoUser user in users)
            {
                callbackData.Add(new KeyValuePair<string, string>($"{user.FirstName} {user.LastName}", ToDoItemCallbackDto.FromString($"selectedUser|{user.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
            context.CurrentStep = "SelectUser";
            return ScenarioResult.Transition;
        }
        private async Task<ScenarioResult> NextPageWithSelectUser(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
            string callback = context.Data["Callback"].ToString();
            if (callback.StartsWith("selectedUser|"))
            {
                int templateId = Int32.Parse(context.Data["TemplateId"].ToString());
                int userId = ToDoItemCallbackDto.FromString(callback).ToDoItemId;
                return await CreateScheduleForUser(botClient, context, message, templateId, userId, ct);
            }
            IReadOnlyList<ToDoUser> users = await _userService.GetAllUsers(ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (ToDoUser user in users)
            {
                callbackData.Add(new KeyValuePair<string, string>($"{user.FirstName} {user.LastName}", ToDoItemCallbackDto.FromString($"selectedUser|{user.Id}").ToString()));
            }
            context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString($"{callback}")), cancellationToken: ct);
            context.CurrentStep = "SelectUser";
            return ScenarioResult.Transition;
        }
        private async Task<ScenarioResult> CreateScheduleForUser(ITelegramBotClient botClient, ScenarioContext context, Message message, int templateId, int userId, CancellationToken ct)
        {
            UserSchedule? userSchedule = await _scheduleRequestService.GetActiveScheduleByUserAsync(userId, ct);
            if (userSchedule != null)
            {
                await botClient.SendMessage(userSchedule.User.TelegramId, "У этого пользователя уже есть активный график!👤📅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(userSchedule.User.Role), cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            ToDoUser assignedById = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
            ScheduleTemplate readyTemplate = await _scheduleRequestService.GetTemplateAsync(templateId, ct);
            userSchedule = new UserSchedule(userId, assignedById.Id, readyTemplate.Id);
            userSchedule = await _scheduleRequestService.InsertScheduleAsync(userSchedule, readyTemplate, ct);
            await botClient.EditMessageText(message.Chat, message.MessageId, "Операция завершена", cancellationToken: ct);
            await botClient.SendMessage(message.Chat, "График для пользователя успешно составлен!📅✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(assignedById.Role), cancellationToken: ct);
            await botClient.SendMessage(userSchedule.User.TelegramId, $"{userSchedule.User.FirstName}, для вас создан {(userSchedule.Template.Type == 0?"линейный":"цикличный")} график с {userSchedule.StartDate.ToShortDateString()} по {userSchedule.EndDate.ToShortDateString()}.", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}