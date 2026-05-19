using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Core.Scenarios
{
    internal class Add_Request : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Add_Request(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Add_Request;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    await botClient.SendMessage(message.Chat, "Опишите причину заявки✍️", replyMarkup:MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    context.CurrentStep = "Reason";
                    return ScenarioResult.Transition;
                case "Reason":
                    if (context.Data.ContainsKey("Callback"))
                    {
                        context.Data.Remove("Callback");
                        await botClient.SendMessage(message.Chat, "Принимается только текст. Опишите причину заявки!", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.From.Id, ct);
                    await _scheduleRequestService.InsertRequestAsync(currentUser.Id, message.Text, ct);
                    await botClient.SendMessage(message.Chat, $"{currentUser.FirstName}, Заявка добавлена.✅ Ожидайте ответа", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}