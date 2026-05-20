using MyShift.Core.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Helpers
{
    internal static class MarkupManager
    {
        /// <summary>
        /// Задать команды меню для пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="role"></param>
        /// <param name="chat"></param>
        /// <param name="ct"></param>
        public static void SetCommand(ITelegramBotClient botClient, Role role, Chat chat, CancellationToken ct)
        {
            SetCommandChat(botClient, role, chat.Id, ct);
        }
        /// <summary>
        /// Задать команды меню для пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="role"></param>
        /// <param name="chat"></param>
        /// <param name="ct"></param>
        public static void SetCommand(ITelegramBotClient botClient, Role role, long chat, CancellationToken ct)
        {
            SetCommandChat(botClient, role, chat, ct);
        }
        private static void SetCommandChat(ITelegramBotClient botClient, Role role, long chat, CancellationToken ct)
        {
            switch (role)
            {
                case Role.User:
                    botClient.SetMyCommands(new List<BotCommand>()
                    {
                        new BotCommand("help","помощь"),
                        new BotCommand("schedule","показывает текущий график"),
                        new BotCommand("add_request","создаёт заявку на смену расписания"),
                        new BotCommand("requests","выводит список заявок")
                    },new BotCommandScopeChat()
                    {
                        ChatId = chat
                    },cancellationToken: ct);
                    break;
                case Role.Operator:
                    botClient.SetMyCommands(new List<BotCommand>()
                    {
                        new BotCommand("help","Помощь"),
                        new BotCommand("edit_schedule","редактирование графиков"),
                        new BotCommand("schedule","показывает текущий график"),
                        new BotCommand("add_request","создаёт заявку на смену расписания"),
                        new BotCommand("requests","выводит список заявок")
                    }, new BotCommandScopeChat()
                    {
                        ChatId = chat
                    }, cancellationToken: ct);
                    break;
                case Role.Administrator:
                    botClient.SetMyCommands(new List<BotCommand>()
                    {
                        new BotCommand("help","Помощь"),
                        new BotCommand("create_template","процесс создания шаблона графиков"),
                        new BotCommand("create_schedule","процесс создания графика для пользователя"),
                        new BotCommand("edit_schedule","редактирование графиков"),
                        new BotCommand("schedule","показывает текущий график"),
                        new BotCommand("add_request","создаёт заявку на смену расписания"),
                        new BotCommand("edit_role","смена ролей"),
                        new BotCommand("requests","выводит список заявок")
                    }, new BotCommandScopeChat()
                    {
                        ChatId = chat
                    }, cancellationToken: ct);
                    break;
                case Role.SuperAdministrator:
                    botClient.SetMyCommands(new List<BotCommand>()
                    {
                        new BotCommand("help","Помощь"),
                        new BotCommand("create_template","процесс создания шаблона графиков"),
                        new BotCommand("create_schedule","процесс создания графика для пользователя"),
                        new BotCommand("edit_schedule","редактирование графиков"),
                        new BotCommand("schedule","показывает текущий график"),
                        new BotCommand("add_request","создаёт заявку на смену расписания"),
                        new BotCommand("edit_role","смена ролей"),
                        new BotCommand("requests","выводит список заявок")
                    }, new BotCommandScopeChat()
                    {
                        ChatId = chat
                    }, cancellationToken: ct);
                    break;
            }
        }
        /// <summary>
        /// Создать стандартную клавиатуру
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static ReplyKeyboardMarkup SetStandartKeyboardButtonList(Role role)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup();
            switch (role)
            {
                case Role.User:
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/schedule📋"),
                        new KeyboardButton("/add_request✍️"),
                        new KeyboardButton("/requests📋")
                    });
                    return replyKeyboardMarkup;
                case Role.Operator:
                    return replyKeyboardMarkup;
                case Role.Administrator:
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/create_template"),
                        new KeyboardButton("/create_schedule"),
                        new KeyboardButton("/edit_schedule"),
                        new KeyboardButton("/edit_role"),
                    });
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/schedule"),
                        new KeyboardButton("/add_request"),
                        new KeyboardButton("/requests")
                    });
                    return replyKeyboardMarkup;
                case Role.SuperAdministrator:
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/create_template"),
                        new KeyboardButton("/create_schedule"),
                        new KeyboardButton("/edit_schedule"),
                        new KeyboardButton("/edit_role"),
                    });
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/schedule"),
                        new KeyboardButton("/add_request"),
                        new KeyboardButton("/requests")
                    });
                    return replyKeyboardMarkup;
                default:
                    replyKeyboardMarkup.AddNewRow(new KeyboardButton[]
                    {
                        new KeyboardButton("/start▶️"),
                        new KeyboardButton("/help🆘")
                    });
                    return replyKeyboardMarkup;
            }
        }
        /// <summary>
        /// Создать клавиатуру для отмены сценария
        /// </summary>
        /// <returns></returns>
        public static ReplyKeyboardMarkup SetKeyboardCancel()
        {
            return new ReplyKeyboardMarkup(new KeyboardButton("/cancel"));
        }
    }
}