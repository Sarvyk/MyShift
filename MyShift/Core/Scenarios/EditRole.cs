using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.DTO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Scenarios
{
    internal class EditRole : IScenario
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        public EditRole(IUserService userService, IScheduleRequestService scheduleRequestService)
        {
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Edit_Role;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch(context.CurrentStep)
            {
                case null:
                    ToDoUser me = await _userService.GetUserByTelegramIdAsync(long.Parse(context.Data["TelegramUserId"].ToString()), ct);
                    IReadOnlyList<ToDoUser> users = null;
                    if (me.Role != Role.SuperAdministrator)
                        users = (await _userService.GetAllUsers(ct)).Where(user => user.Role<me.Role && user.Id != me.Id).ToList();
                    else
                        users = (await _userService.GetAllUsers(ct)).Where(user => user.Id != me.Id).ToList();
                    if (users.Count == 0)
                    {
                        await botClient.SendMessage(message.Chat, "Пользователи не найдены!", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    var callbackData = new List<KeyValuePair<string, string>>();
                    foreach (ToDoUser user in users)
                    {
                        callbackData.Add(new KeyValuePair<string, string>($"{user.Id}){user.FirstName} {user.LastName}({user.Role})", ToDoItemCallbackDto.FromString($"showUser|{user.Id}").ToString()));
                    }
                    await botClient.SendMessage(message.Chat, "Выберите пользователя", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
                    context.CurrentStep = "selectUser";
                    return ScenarioResult.Transition;
                case "selectUser":
                    me = await _userService.GetUserByTelegramIdAsync(long.Parse(context.Data["TelegramUserId"].ToString()), ct);
                    users = (await _userService.GetAllUsers(ct)).Where(user => user.Role < me.Role && user.Id != me.Id).ToList();
                    callbackData = new List<KeyValuePair<string, string>>();
                    if (!context.Data["Callback"].ToString().StartsWith("showUser|"))
                    {
                        foreach (ToDoUser user in users)
                        {
                            callbackData.Add(new KeyValuePair<string, string>($"{user.Id}){user.FirstName} {user.LastName}({user.Role})", ToDoItemCallbackDto.FromString($"showUser|{user.Id}").ToString()));
                        }
                    }
                    Message callbackMessage = (Message)context.Data["CallbackMessage"];
                    if (context.Data["Callback"].ToString().StartsWith("showUserPageNext"))
                    {
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                        await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Выберите пользователя", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    context.Data.Add("userId", context.Data["Callback"].ToString());
                    List<Role> roles = new List<Role>();
                    if(me.Role == Role.SuperAdministrator)
                      roles = Enum.GetValues<Role>().ToList();
                    else
                        roles = Enum.GetValues<Role>().Where(r => ((int)r) < (int)me.Role).ToList();
                    InlineKeyboardButton[] buttons = new InlineKeyboardButton[roles.Count];
                    int i = 0;
                    foreach (Role role in roles)
                    {
                        buttons[i++] = new InlineKeyboardButton(role.ToString(), ((int)role).ToString());
                    }
                    await botClient.EditMessageText(callbackMessage.Chat, callbackMessage.MessageId, "Выберите роль", replyMarkup: new InlineKeyboardMarkup().AddNewRow(buttons), cancellationToken: ct);
                    context.CurrentStep = "selectedRole";
                    return ScenarioResult.Transition;
                case "selectedRole":
                    int userId = ToDoItemCallbackDto.FromString(context.Data["userId"].ToString()).ToDoItemId;
                    Role roleResult = (Role)Int32.Parse(context.Data["Callback"].ToString());
                    await _userService.SetRole(userId, roleResult, ct);
                    await botClient.SendMessage(message.Chat, $"Роль у пользователя \"{userId}\" успешно изменена на \"{roleResult}\"",  cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}