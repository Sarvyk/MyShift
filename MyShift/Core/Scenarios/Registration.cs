using MyShift.Core.Enums;
using MyShift.Core.Extensions;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MyShift.Core.Scenarios
{
    internal class Registration : IScenario
    {
        private readonly IUserService _userService;
        public Registration(IUserService userService)
        {
            _userService = userService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.Registration;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch(context.CurrentStep)
            {
                case null:
                    Role[] roles = Enum.GetValues<Role>().Where(r => !r.HasFlag(Role.SuperAdministrator)).ToArray();
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                    int maxInRow = 2;
                    InlineKeyboardButton[] keyboardButtons = new InlineKeyboardButton[maxInRow];
                    for (int i = 0, roleposition = 0; i < roles.Length; i++, roleposition++)
                    {
                        //Создание кнопок по 2 кнопки роли на строку
                        keyboardButtons[roleposition] = new InlineKeyboardButton(roles[i].GetDisplayName(), ((int)roles[i]).ToString());
                        if (roleposition == maxInRow-1)
                        {
                            roleposition = -1;
                            inlineKeyboard.AddNewRow(keyboardButtons);
                            keyboardButtons = new InlineKeyboardButton[maxInRow];
                        }
                    }
                    await botClient.SendMessage(message.Chat, "Процесс регистрации нового пользователя", replyMarkup: MarkupManager.SetKeyboardCancel(), cancellationToken: ct);
                    context.Data["currentMessage"] = await botClient.SendMessage(message.Chat, "Выберите роль", replyMarkup: inlineKeyboard, cancellationToken: ct);
                    context.CurrentStep = "SelectedRole";
                    return ScenarioResult.Transition;
                case "SelectedRole":
                    Validator.ValidateCurrentMessage((Message)context.Data["currentMessage"], message, 0);
                    Role role = (Role)Int32.Parse(context.Data["Callback"].ToString());
                    int userRegId = Int32.Parse(context.Data["userId"].ToString());
                    ToDoUser currentUser = await _userService.GetUserByTelegramIdAsync(message.Chat.Id, ct);
                    ToDoUser userRegistration = await _userService.GetUserAsync(userRegId, ct);
                    await _userService.SetRoleAsync(userRegistration.Id, role, ct);
                    await botClient.SendMessage(message.Chat, $"Пользователелю выдана роль \"{role.GetDisplayName()}\" успешно добавлен.", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(currentUser.Role), cancellationToken: ct);
                    await botClient.SendMessage(userRegistration.TelegramId, $"Ваша регистрация завершена. Вам выдана роль \"{role.GetDisplayName()}\"", replyMarkup:MarkupManager.SetStandartKeyboardButtonList(role), cancellationToken: ct);
                    MarkupManager.SetCommand(botClient, role, userRegistration.TelegramId, ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}