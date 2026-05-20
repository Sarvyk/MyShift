using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.Core.Services;
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
    internal class Cancel_Request : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Cancel_Request(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Cancel_Request;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch(context.CurrentStep)
            {
                case null:
                    context.Data.Add("requestId", context.Data["Callback"]);
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Введите причину отказа для пользователя", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    context.CurrentStep = "EnterMessage";
                    return ScenarioResult.Transition;
                case "EnterMessage":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 1);
                    int requestId = Int32.Parse(context.Data["requestId"].ToString());
                    string messageToUser = message.Text;
                    ToDoUser user = await _userService.GetUserByTelegramIdAsync(message.From.Id, ct);
                    Request request = await _scheduleRequestService.RejectRequestAsync(requestId, user.Id, messageToUser, ct);
                    await botClient.SendMessage(message.From.Id, "Заявка отклонена", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(user.Role), cancellationToken: ct);
                    await botClient.SendMessage(request.Creator.TelegramId, $"Ваша заявка отклонена. Причина:\r\n---{messageToUser}---", cancellationToken:ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}
