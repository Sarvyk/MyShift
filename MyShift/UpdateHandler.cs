using Microsoft.EntityFrameworkCore;
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
                        botClient.SendMessage(update.Message.Chat, StartCommand(update.Message.From));
                        break;
                    //case "/GetData":
                    //    botClient.SendMessage(update.Message.Chat, $"Данные пользователя для работы: FirstName {update.Message.From.FirstName}; Lastname {update.Message.From.LastName}; Username {update.Message.From.Username}; UserId {update.Message.From.Id}; isBot {update.Message.From.IsBot}");
                    //    break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Такой команды не существует.");
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
    }
}