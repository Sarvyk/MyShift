using MyShift.Core.Data;
using MyShift.Core.Dialogs;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Services;
using MyShift.Repositories;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MyShift
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly Dictionary<long, IDialog> _waitMessage = [];
        public UpdateHandler()
        {
            var sqlContext = new SqLiteDbContext();
            var userRepository = new UserRepository(sqlContext);
            var userService = new UserService(userRepository);
            var requestRepository = new RequestRepository(sqlContext);
            var scheduleRepository = new ScheduleRepository(sqlContext);
            var scheduleRequestService = new ScheduleRequestService(requestRepository, scheduleRepository);
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
        }
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {//добавил пока что обычный вывод, пока не знаю что ещё тут можно сделать.
            Console.WriteLine(exception.Message);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message.Text.StartsWith('/'))
                {
                    ResetWaitMessage(update.Message.Chat.Id);//если у нас есть ожидание диалога, то при попадании в эту секцию кода мы удаляем это ожидание т.к. ввелась команда.
                    switch (update.Message.Text)
                    {
                        case "/start":
                            await StartCommand(botClient, update, cancellationToken);
                            await HelpCommand(botClient, update, cancellationToken);
                            break;
                        case "/help":
                            await HelpCommand(botClient, update, cancellationToken);
                            break;
                        case "/график":
                            if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                            {

                            }
                            break;
                        case "/создать график":
                            IReadOnlyList<ScheduleTemplate> templates = await _scheduleRequestService.GetAllTemplates();
                            if (templates.Count == 0)
                            {
                                await botClient.SendMessage(update.Message.Chat, "Отсутствуют шаблоны графиков!");
                                return;
                            }
                            await botClient.SendMessage(update.Message.Chat, "Для создания графика, необходимо заполнить данные. Выберите кому назначаете график.");
                            IReadOnlyList<ToDoUser> users = await _userService.GetAllUsers(cancellationToken);
                            await PrintUserList(botClient, update, users, cancellationToken);
                            ToDoUser user = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, cancellationToken);
                            await AddNewDialog(update.Message.Chat.Id, new DialogCreateSchedule(botClient, update, templates, users, new ScheduleBuilder(user),_scheduleRequestService));
                            break;
                        case "/создать шаблон":
                            await botClient.SendMessage(update.Message.Chat, "Для создания шаблона необъодимо ввести имя шаблона, время и дни работы. Введите название шаблона.");
                            user = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, cancellationToken);
                            await AddNewDialog(update.Message.Chat.Id, new DialogCreateTemplate(botClient, update,new ScheduleBuilder(user), _scheduleRequestService));
                            break;
                        case string a when a.StartsWith("/добавить заявку"):
                            if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                            {
                                await CreateRequest(botClient, update, a.Replace("/добавить заявку", "").Trim(), cancellationToken);
                            }
                            break;
                        case "/заявки":
                            if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                            {
                                await GetRequests(botClient, update, cancellationToken);
                            }
                            break;
                        case string a when a.StartsWith("/удалить заявку"):
                            if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                            {
                                await DeleteRequest(botClient, update, a.Replace("/удалить заявку", "").Trim(), cancellationToken);
                            }
                            break;
                        default:
                            await botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.", cancellationToken: cancellationToken);
                            await HelpCommand(botClient, update, cancellationToken);
                            break;
                    }
                }
                else if(_waitMessage.ContainsKey(update.Message.Chat.Id))
                {
                    bool result = await _waitMessage[update.Message.Chat.Id].NextStep(update.Message.Text, cancellationToken);
                    if(result)
                        _waitMessage.Remove(update.Message.Chat.Id);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                await botClient.SendMessage(update.Message.Chat, ex.Message, cancellationToken: cancellationToken);
            }
            catch (FormatException ex)
            {
                await botClient.SendMessage(update.Message.Chat, ex.Message, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, cancellationToken);
            }
        }
        private async Task PrintUserList(ITelegramBotClient botClient, Update update, IReadOnlyList<ToDoUser> users, CancellationToken ct)
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (ToDoUser user in users)
            {
                string nickname = user.UserName != null ? $"{user.UserName};" : "";
                string firstname = user.FirstName != null ? $"{user.FirstName};" : "";
                string lastname = user.LastName != null ? $"{user.LastName};" : "";
                sb.AppendLine($"{i++}){nickname}; {lastname} {firstname}");
            }
            await botClient.SendMessage(update.Message.Chat, $"{sb.ToString()}", cancellationToken:ct);
        }
        private void ResetWaitMessage(long chatId)
        {
            if(_waitMessage.ContainsKey(chatId))
                _waitMessage.Remove(chatId);
        }

        private async Task AddNewDialog(long chatId, IDialog dialog)
        {
            if(_waitMessage.ContainsKey(chatId))
                _waitMessage.Remove(chatId);
            else
                _waitMessage.Add(chatId, dialog);
        }
        private async Task DeleteRequest(ITelegramBotClient botClient, Update update, string number, CancellationToken ct)
        {
            ToDoUser? user = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id,ct);
            await _scheduleRequestService.DeleteRequestAsync(user.Id, number, ct);
            await botClient.SendMessage(update.Message.Chat, $"Запись №{number} удалена!",cancellationToken:ct);
        }

        private async Task StartCommand(ITelegramBotClient botClient,Update update, CancellationToken ct)
        {
            ToDoUser? toDoUser = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            if (toDoUser != null) 
            {
                await botClient.SendMessage(update.Message.Chat,"Бот уже запущен!",cancellationToken: ct);
            }
            else
            {
                toDoUser = await _userService.RegisterUserAsync(update.Message.Chat.Id, update.Message.From, ct);
                await botClient.SendMessage(update.Message.Chat, $"{toDoUser.FirstName}, добро пожаловать в бот \"Мой график\"!", cancellationToken: ct);
            }
        }
        private void CreateTemplateSchedule()
        {

        }
        private string GetSchedule(ToDoUser toDoUser)
        {
            
            return "";
        }
        private async Task CreateRequest(ITelegramBotClient botClient, Update update, string message, CancellationToken ct)
        {
            ToDoUser? user = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            await _scheduleRequestService.InsertRequestAsync(user.Id, message, ct);
            await botClient.SendMessage(update.Message.Chat,"Заявка добавлена", cancellationToken:ct);
        }
        private async Task GetRequests(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            StringBuilder Answer = new();
            var queryResult = await _scheduleRequestService.GetRequestsAsync(user.Id, ct);
            if (queryResult.Count > 0)
            {
                Answer.AppendLine($"{user.FirstName}, вот список ваших заявок:");
                foreach (Request req in await _scheduleRequestService.GetRequestsAsync(user.Id,ct))
                {
                    Answer.AppendLine($"{req.Id}) Сообщение:{req.Message}; Статус:{req.Status.GetDisplayName()}");
                }
                await botClient.SendMessage(update.Message.Chat,Answer.ToString(),cancellationToken:ct);
            }
            else
                await botClient.SendMessage(update.Message.Chat, $"{user.FirstName}, у вас нет заявок!", cancellationToken:ct);
        }
        private async Task<bool> CheckCredentials(User user, Role roles, CancellationToken ct)
        {
            ToDoUser? toDoUser = await _userService.GetUserByTelegramIdAsync(user.Id,ct);
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
        private async Task HelpCommand(ITelegramBotClient botClient,Update update, CancellationToken ct)
        {
            ToDoUser? toDoUser = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            if (toDoUser != null)
            {
                if (toDoUser.Role == Role.Administrator)
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/создать шаблон - процесс создания шаблона графиков;
/создать график - процесс создания графика для пользователя
/график - показывает текущий график;
/добавить заявку [описание] - создаёт заявку на смену расписания;
/заявки - выводит список заявок;
/удалить заявку [номер] - удаляет заявку по заданному номеру", cancellationToken:ct);
                else if (toDoUser.Role == Role.Moderator)
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:заглушка", cancellationToken: ct);
                else
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/график - показывает текущий график;
/добавить заявку [описание] - создаёт заявку на смену расписания;
/заявки - выводит список заявок;
/удалить заявку [номер] - удаляет заявку по заданному номеру", cancellationToken: ct);
            }
            else
                await botClient.SendMessage(update.Message.Chat, @"Вот список доступных комманд:/start, /help", cancellationToken: ct);
        }
    }
}