using Microsoft.EntityFrameworkCore;
using MyShift.Enums;
using MyShift.Models;
using MyShift.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace MyShift
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        public UpdateHandler(IUserService service)
        {
            _userService = service;
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Message.Text)
            {
                case "/start":
                    botClient.SendMessage(update.Message.Chat, $"{StartCommand(update.Message.From)}\r\n{HelpCommand(update.Message.From)}");
                    break;
                case "/help":
                    botClient.SendMessage(update.Message.Chat, HelpCommand(update.Message.From));
                    break;
                case "/график":
                    break;
                case string a when a.IndexOf("/изменить") == 0:
                    if (CheckCredentials(update.Message.From, Role.Moderator | Role.Administrator,out string answer))
                    {
                        Console.WriteLine(answer);
                        botClient.SendMessage(update.Message.Chat, string.IsNullOrWhiteSpace(answer) ? "заглушка" : answer);
                    }
                    break;
                case "/заявки":
                    break;
                default:
                    botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.\r\n{HelpCommand(update.Message.From)}");
                    break;
            }
            return Task.CompletedTask;
        }
        private string StartCommand(User user)
        {
            ToDoUser? toDoUser = _userService.GetUserByTelegramId(user.Id);
            if (toDoUser != null) 
            {
                return "Бот уже запущен!";
            }
            else
            {
                toDoUser = _userService.RegisterUser(user);
                return $"{toDoUser.UserName}, добро пожаловать в бот \"Мой график\"!";
            }
        }
        //private string CreateRequest()
        //{

        //}
        private bool CheckCredentials(User user, Role roles, out string answer)
        {
            ToDoUser? toDoUser = _userService.GetUserByTelegramId(user.Id);
            if(toDoUser != null)
            {
                if (roles.HasFlag(toDoUser.Role))
                {
                    answer = "";
                    return true;
                }
                else
                {
                    answer = "Вам не доступна эта команда!";
                    return false;
                }
            }
            else
            {
                answer = "Вы не зарегистрированы. Введите /start";
                return false;
            }
        }
        private string HelpCommand(User user)
        {
            ToDoUser? toDoUser = _userService.GetUserByTelegramId(user.Id);
            if (toDoUser != null)
            {
                if (toDoUser.Role == Role.Administrator)
                    return @"Список команд:заглушка";
                else if (toDoUser.Role == Role.Moderator)
                    return @"Список команд:заглушка";
                else
                    return @"Список команд:
/график - показывает текущий график; 
/изменить [описание] - создаёт заявку на смену расписания,
/заявки - выводит список заявок";
            }
            else
                return @"Вот список доступных комманд:/start, /help";
        }
    }
}