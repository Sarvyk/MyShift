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
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            ToDoUser toDoUser;
            try
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
                        if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                        {
                            //botClient.SendMessage(update.Message.Chat, CreateRequest(toDoUser));
                        }
                        break;
                    case string a when a.IndexOf("/добавить заявку") == 0:
                        if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                        {
                            botClient.SendMessage(update.Message.Chat, CreateRequest(toDoUser, a.Replace("/добавить заявку", "").Trim()));
                        }
                        break;

                    case "/заявки":
                        if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                        {
                            botClient.SendMessage(update.Message.Chat, GetRequests(toDoUser));
                        }
                        break;
                    case string a when a.IndexOf("/удалить заявку") == 0:
                        if (CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, out toDoUser))
                        {
                            botClient.SendMessage(update.Message.Chat, DeleteRequest(toDoUser, a.Replace("/удалить заявку", "").Trim()));
                        }
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.\r\n{HelpCommand(update.Message.From)}");
                        break;
                }
            }
            catch (ArgumentException argEx)
            {
                botClient.SendMessage(update.Message.Chat, argEx.Message);
            }
            catch(Exception ex)
            {
                await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, cancellationToken);
            }
        }

        private string DeleteRequest(ToDoUser toDoUser, string number)
        {
            _scheduleRequestService.DeleteRequestAsync(toDoUser.Id, number);
            return $"Запись №{number} удалена!";
        }

        private string StartCommand(User user)
        {
            ToDoUser? toDoUser = _userService.GetUserByTelegramIdAsync(user.Id).Result;
            if (toDoUser != null) 
            {
                return "Бот уже запущен!";
            }
            else
            {
                toDoUser = _userService.RegisterUserAsync(user).Result;
                return $"{toDoUser.UserName}, добро пожаловать в бот \"Мой график\"!";
            }
        }
        private string GetSchedule(ToDoUser toDoUser)
        {
            
            return "";
        }
        private string CreateRequest(ToDoUser toDoUser, string message)
        {
            _scheduleRequestService.CreateRequestAsync(toDoUser.Id, message);
            return "Заявка добавлена";
        }
        private string GetRequests(ToDoUser toDoUser)
        {
            StringBuilder Answer = new StringBuilder();
            var queryResult = _scheduleRequestService.GetRequestsAsync(toDoUser.Id).Result;
            if (queryResult.Count > 0)
            {
                Answer.AppendLine($"{toDoUser.FirstName}, вот список ваших заявок:");
                foreach (Request req in _scheduleRequestService.GetRequestsAsync(toDoUser.Id).Result)
                {
                    Answer.AppendLine($"{req.Id}) текст:{req.Message}; статус:{req.Status.ToString()}");
                }
                return Answer.ToString();
            }
            else 
                return $"{toDoUser.FirstName}, у вас нет заявок!";
        }
        private bool CheckCredentials(User user, Role roles, out ToDoUser toDoUser)
        {
            toDoUser = _userService.GetUserByTelegramIdAsync(user.Id).Result;
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
            ToDoUser? toDoUser = _userService.GetUserByTelegramIdAsync(user.Id).Result;
            if (toDoUser != null)
            {
                if (toDoUser.Role == Role.Administrator)
                    return @"Список команд:заглушка";
                else if (toDoUser.Role == Role.Moderator)
                    return @"Список команд:заглушка";
                else
                    return @"Список команд:
/график - показывает текущий график; 
/добавить заявку [описание] - создаёт заявку на смену расписания,
/заявки - выводит список заявок
/удалить заявку [номер] - удаляет заявку по заданному номеру";
            }
            else
                return @"Вот список доступных комманд:/start, /help";
        }
        private void Validate()
        {

        }
    }
}