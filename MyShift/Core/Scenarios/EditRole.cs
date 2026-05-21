using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.DTO;
using System.Data;
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
            switch (context.CurrentStep)
            {
                case null:
                    ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
                    IReadOnlyList<ToDoUser> users = null;
                    if (currentUser.Role != Role.SuperAdministrator)
                        users = (await _userService.GetAllUsersAsync(ct)).Where(user => user.Role<currentUser.Role && user.Id != currentUser.Id).ToList();
                    else
                        users = (await _userService.GetAllUsersAsync(ct)).Where(user => user.Id != currentUser.Id).ToList();
                    if (users.Count == 0)
                    {
                        await botClient.SendMessage(message.Chat, "Пользователи не найдены!🔍❌", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    var callbackData = new List<KeyValuePair<string, string>>();
                    foreach (ToDoUser user in users)
                    {
                        callbackData.Add(new KeyValuePair<string, string>($"{user.Id}){user.FirstName} {user.LastName}({user.Role.GetDisplayName()})", ToDoItemCallbackDto.FromString($"showUser|{user.Id}").ToString()));
                    }
                    await botClient.SendMessage(message.Chat, "Процесс смены роли", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showUserPageNext||0")), cancellationToken: ct);
                    context.CurrentStep = "selectUser";
                    return ScenarioResult.Transition;
                case "selectUser":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                    string callback = context.Data["Callback"].ToString();
                    currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
                    users = (await _userService.GetAllUsersAsync(ct)).Where(user => user.Role < currentUser.Role && user.Id != currentUser.Id).ToList();
                    callbackData = new List<KeyValuePair<string, string>>();
                    if (!callback.StartsWith("showUser|"))
                    {
                        foreach (ToDoUser user in users)
                        {
                            callbackData.Add(new KeyValuePair<string, string>($"{user.Id}){user.FirstName} {user.LastName}({user.Role.GetDisplayName()})", ToDoItemCallbackDto.FromString($"showUser|{user.Id}").ToString()));
                        }
                    }
                    if (callback.StartsWith("showUserPageNext"))
                    {
                        // Если мы получаем в колбеке данные о кнопках смены страниц, то попадаем сюда и меняем страницу, а дальше сохраняем шаг тем же.
                        context.Data["currentMessage"] = await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите пользователя👥", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString(context.Data["Callback"].ToString())), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    context.Data.Add("userId", callback);
                    List<Role> roles = new List<Role>();
                    if(currentUser.Role == Role.SuperAdministrator)
                      roles = Enum.GetValues<Role>().ToList();
                    else
                        roles = Enum.GetValues<Role>().Where(r => ((int)r) < (int)currentUser.Role).ToList();
                    InlineKeyboardMarkup markup = new InlineKeyboardMarkup();
                    foreach (Role role in roles)
                    {
                        markup.AddNewRow(new InlineKeyboardButton[] { new InlineKeyboardButton(role.GetDisplayName(), ((int)role).ToString()) });
                    }
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "🎭Выберите роль🎭", replyMarkup: markup, cancellationToken: ct);
                    context.CurrentStep = "selectedRole";
                    return ScenarioResult.Transition;
                case "selectedRole":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                    callback = context.Data["Callback"].ToString();
                    currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
                    ToDoUser userEditRole = await _userService.GetUserAsync(ToDoItemCallbackDto.FromString(context.Data["userId"].ToString()).ToDoItemId, ct);
                    Role roleResult = (Role)Int32.Parse(callback);
                    await _userService.SetRoleAsync(userEditRole.Id, roleResult, ct);
                    await botClient.SendMessage(message.Chat, $"Роль у пользователя \"{userEditRole.Id}\" успешно изменена на\r\n\"{roleResult.GetDisplayName()}\"✅", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                    await botClient.SendMessage(userEditRole.TelegramId, $"Ваша роль изменена на {roleResult.GetDisplayName()}", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(roleResult),cancellationToken:ct);
                    MarkupManager.SetCommand(botClient, roleResult, message.Chat, ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}