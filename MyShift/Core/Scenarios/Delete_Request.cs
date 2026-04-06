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
    internal class Requests : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public Requests(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Requests;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch(context.CurrentStep)
            {
                case null:
                    context.CurrentStep = "Reason";
                    ToDoUser user = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(message.From.Id, ct)).Id, ct);
                    context.Data.Add("User", user);
                    IReadOnlyList<Request> requests = await _scheduleRequestService.GetRequestsAsync(user.Id, ct);
                    List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                    foreach (Request request in requests)
                    {
                        callbackData.Add(new KeyValuePair<string, string>(request.CreatedAt.ToString("dd MMM yyyyy года"), ToDoItemCallbackDto.FromString($"showtask|{request.Id}").ToString()));
                    }
                    if(callbackData.Count == 0)
                    {
                        await botClient.SendMessage(message.Chat, "Вы ещё не подавали заявки", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    await botClient.SendMessage(message.Chat, "Выберите заявку, чтобы посмотреть её статус и описание", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("show||0")), cancellationToken: ct);
                    context.CurrentStep = "show";
                    return ScenarioResult.Transition;
                case "":
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}
//List<InlineKeyboardButton[]> listButtons = new List<InlineKeyboardButton[]>();
//foreach (ToDoList list in userLists)
//{
//    listButtons.Add(new[] { new InlineKeyboardButton() { Text = list.Name, CallbackData = ToDoListCallbackDto.FromString($"deletelist|{list.Id}").ToString() } });
//}
//context.Data.Add("Callback", "");//Для переноса ответа.
//await botClient.SendMessage(message.Chat, "Выберете список для удаления:", replyMarkup: new InlineKeyboardMarkup(listButtons), cancellationToken: ct);