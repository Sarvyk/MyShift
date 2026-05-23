using Microsoft.EntityFrameworkCore;
using MyShift.Core.Entities;
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
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Scenarios
{
    internal class Delete_Request : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly INotificationService _notificationService;

        public Delete_Request(IUserService userService, IScheduleRequestService scheduleRequestService, INotificationService notificationService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
            _notificationService = notificationService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Delete_Request;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            Role roleCurrentUser = Role.None;
            switch (context.CurrentStep)
            {
                case null:
                    int requestId = ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId;
                    context.Data.Add("RequestId", requestId);
                    await botClient.SendMessage(message.Chat, "Процесс удаления заявки", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, $"Подтверждаете удаление заявки❓", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("✅Да", "yes"), new InlineKeyboardButton("❌Нет", "no")), cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;
                case "Approve":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                    roleCurrentUser = (await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct)).Role;
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, $"Удаление отменено↩️", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    Request deletingRequest = await _scheduleRequestService.GetRequestAsync(Int32.Parse(context.Data["RequestId"].ToString()), ct);
                    Notification? notification = await _notificationService.GetNotificationByUserIdAndType(deletingRequest.CreatorId, $"Request_{deletingRequest.Id}", ct);
                    await _scheduleRequestService.DeleteRequestAsync(deletingRequest.Id, ct);
                    if(notification != null)
                        await _notificationService.MarkNotified(notification.id, ct);
                    break;
            }
            await botClient.SendMessage(message.Chat, $"Заявка удалена🗑️✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleCurrentUser), cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}