using MyShift.Core.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.Core.Helpers
{
    internal static class MarkupManager
    {
        public static async Task SetCommand(ITelegramBotClient botClient, Role role, Chat chat, CancellationToken ct)
        {
            switch (role)
            {
                case Role.User:
                    await botClient.SetMyCommands(new List<BotCommand>()
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
                case Role.Moderator:
                    await botClient.SetMyCommands(new List<BotCommand>()
                    {
                        
                    }, new BotCommandScopeChat()
                    {
                        ChatId = chat
                    }, cancellationToken: ct);
                    break;
                case Role.Administrator:
                    await botClient.SetMyCommands(new List<BotCommand>()
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
                    await botClient.SetMyCommands(new List<BotCommand>()
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
    }
}