using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    await botClient.SendMessage(message.Chat, "Опишите причину заявки✍️", cancellationToken: ct);
                    context.CurrentStep = "Reason";
                    context.Data.Add("User", await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(message.From.Id, ct)).Id, ct));
                    return ScenarioResult.Transition;
                case "Reason":
                    ToDoUser user = (ToDoUser)context.Data["User"];
                    await _scheduleRequestService.InsertRequestAsync(user.Id, message.Text, ct);
                    await botClient.SendMessage(message.Chat, $"{user.FirstName}, Заявка добавлена.✅ Ожидайте ответа", cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}