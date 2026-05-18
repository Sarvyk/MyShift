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
                    context.Data.Add("userId", context.Data["Callback"]);
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
                    await botClient.EditMessageText(message.Chat, message.MessageId, "Выберите роль", replyMarkup: inlineKeyboard, cancellationToken: ct);
                    context.CurrentStep = "SelectedRole";
                    return ScenarioResult.Transition;
                case "SelectedRole":
                    Role role = (Role)Int32.Parse(context.Data["Callback"].ToString());
                    await _userService.SetRole(Int32.Parse(context.Data["userId"].ToString()), role, ct);
                    await botClient.SendMessage(message.Chat, $"Пользователелю выдана роль \"{role.GetDisplayName()}\" успешно добавлен.", cancellationToken: ct);
                    await botClient.SendMessage(Int32.Parse(context.Data["userId"].ToString()), $"Ваша регистрация завершена. Вам выдана роль \"{role.GetDisplayName()}\"", cancellationToken: ct);
                    MarkupManager.SetCommand(botClient, role, long.Parse(context.Data["userId"].ToString()), ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}