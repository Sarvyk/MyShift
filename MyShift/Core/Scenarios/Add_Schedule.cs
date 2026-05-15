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
        {//как-то мне не очень нравится этот вариант решения. Попозже возможно вернусь к этому кусочку
            Role roleCurrentUser = (Role)Int32.Parse(context.Data["userRole"].ToString());
            if (!context.Data.ContainsKey("Template"))
            {
                if (!context.Data["Callback"].ToString().Contains("selectedTemplate"))
                {
                    IReadOnlyList<ScheduleTemplate> templates = await _scheduleRequestService.GetAllTemplatesAsync(ct);
                    if (templates.Count == 0)
                    {
                        await botClient.SendMessage(message.Chat, "Действующие шаблоны не найдены!🔍❌", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    var callbackData = new List<KeyValuePair<string, string>>();
                    foreach (ScheduleTemplate template in templates)
                    {
                        callbackData.Add(new KeyValuePair<string, string>(template.Name, ToDoItemCallbackDto.FromString($"selectedTemplate|{template.Id}").ToString()));
                    }
                    await botClient.SendMessage(message.Chat, "Процесс создания графика", replyMarkup:MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    if (context.Data["Callback"].ToString() == "")
                    {
                        await botClient.SendMessage(message.Chat, "Выберите шаблон📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showTemplate||0")), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    else if (context.Data["Callback"].ToString().Contains("showTemplate"))
                    {
                        await botClient.EditMessageText(context.Data["ChatId"].ToString(), message.MessageId, "Выберите шаблон", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString($"{context.Data["Callback"].ToString()}")), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                }
                context.Data.Add("Template", await _scheduleRequestService.GetTemplateAsync(ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId, ct));
                context.Data["Callback"] = string.Empty;
            }
            if (!context.Data.ContainsKey("userId"))
            {
                if (!context.Data["Callback"].ToString().Contains("selectedUser"))
                {
                    IReadOnlyList<ToDoUser> users = await _userService.GetAllUsers(ct);
                    var callbackData = new List<KeyValuePair<string, string>>();
                    foreach (ToDoUser user in users)
                    {
                        callbackData.Add(new KeyValuePair<string, string>($"{user.FirstName} {user.LastName}", ToDoItemCallbackDto.FromString($"selectedUser|{user.Id}").ToString()));
                    }
                    if (context.Data["Callback"] == "")
                    {
                        await botClient.EditMessageText(context.Data["ChatId"].ToString(), message.MessageId, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUser||0")), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    else if (context.Data["Callback"].ToString().Contains("showUser"))
                    {
                        await botClient.EditMessageText(context.Data["ChatId"].ToString(), message.MessageId, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString($"{context.Data["Callback"].ToString()}")), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                }
                context.Data.Add("userId", context.Data["Callback"].ToString());
            }

            int userId = ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId;
            if((await _scheduleRequestService.GetActiveScheduleByUserAsync(userId,ct)) != null)
            {
                await botClient.SendMessage(context.Data["ChatId"].ToString(), "У этого пользователя уже есть активный график!👤📅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                return ScenarioResult.Completed;
            }
            int assignedById = (await _userService.GetUserByTelegramIdAsync(long.Parse(context.Data["TelegramUserId"].ToString()), ct)).Id;
            ScheduleTemplate readyTemplate = (ScheduleTemplate)context.Data["Template"];
            UserSchedule userSchedule = new UserSchedule(userId, assignedById, readyTemplate.Id);
            await _scheduleRequestService.InsertScheduleAsync(userSchedule, readyTemplate, ct);
            await botClient.SendMessage(context.Data["ChatId"].ToString(), "График для пользователя успешно составлен!📅✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}