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
        private readonly IScheduleRequestService _scheduleRequestService;
        public UpdateHandler(IUserService userService, IScheduleRequestService schReqService)
        {
            _userService = userService;
            _scheduleRequestService = schReqService;
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            ToDoUser toDoUser;
            switch (update.Message.Text)
            {
                case "/start":
                    botClient.SendMessage(update.Message.Chat, $"{StartCommand(update.Message.From)}\r\n{HelpCommand(update.Message.From)}");
                    break;
                case "/help":
                    botClient.SendMessage(update.Message.Chat, HelpCommand(update.Message.From));
                    break;
                case "/график":
                    if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                    {
                        botClient.SendMessage(update.Message.Chat, CreateRequest(toDoUser));
                    }
                    break;
                case string a when a.IndexOf("/изменить") == 0:
                    if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                    {
                        botClient.SendMessage(update.Message.Chat, GetSchedule(toDoUser));
                    }
                    break;

                case "/заявки":
                    if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                    {
                        botClient.SendMessage(update.Message.Chat, GetRequests(toDoUser));
                    }
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
        private string GetSchedule(ToDoUser toDoUser)
        {
            
            return "";
        }
        private string CreateRequest(ToDoUser toDoUser)
        {

            return "";
        }
        private string GetRequests(ToDoUser toDoUser)
        {

            return "";
        }
        private bool CheckCredentials(User user, Role roles, out ToDoUser toDoUser)
        {
            toDoUser = _userService.GetUserByTelegramId(user.Id);
            if(toDoUser != null)
            {
                if (roles.HasFlag(toDoUser.Role))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
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