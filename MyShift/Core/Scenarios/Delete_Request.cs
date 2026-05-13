using Microsoft.EntityFrameworkCore;
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
        private readonly IScheduleRequestService _scheduleRequestService;
        public Delete_Request(IScheduleRequestService scheduleRequestService)
        {
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Delete_Request;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch(context.CurrentStep)
            {
                case null:
                    int requestId = ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId;
                    context.Data.Add("RequestId", requestId);
                    await botClient.SendMessage(message.Chat, $"Подтверждаете удаление заявки❓", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("✅Да", "yes"), new InlineKeyboardButton("❌Нет", "no")), cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;
                case "Approve":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, $"Удаление отменено↩️", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    await _scheduleRequestService.DeleteRequestAsync(Int32.Parse(context.Data["RequestId"].ToString()), ct);
                    break;
            }
            await botClient.SendMessage(message.Chat, $"Заявка удалена🗑️✅", cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}