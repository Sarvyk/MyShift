using MyShift.Core.Data;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.Core.Scenarios;
using MyShift.Core.Scenarios.Enums;
using MyShift.Core.Scenarios.Interfaces;
using MyShift.DTO;
using System.Collections;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly IEnumerable _scenarios;
        private readonly IScenarioContextRepository _scenarioContextRepository;
        public UpdateHandler(IUserService userService, IScheduleRequestService scheduleRequestService, IEnumerable scenarios, IScenarioContextRepository scenarioContextRepository)
        {
            var sqlContext = new SqLiteDbContext();
            _userService = userService;
            _scheduleRequestService = scheduleRequestService;
            _scenarios = scenarios;
            _scenarioContextRepository = scenarioContextRepository;
        }
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {//добавил пока что обычный вывод, пока не знаю что ещё тут можно сделать.
            Console.WriteLine(exception.Message);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        await HandleCallBack(botClient, update, cancellationToken);
                        break;
                    case UpdateType.Message:
                        await HandleMessage(botClient, update, cancellationToken);
                        break;
                    default:
                        //await botClient.SendMessage(update.Message.Chat, "Такой формат пока не поддерживается!", replyMarkup: MarkupManager.SetStandartKeyboardButtonList(), cancellationToken: ct);
                        return;
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

        private async Task HandleMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            ScenarioContext? context;
            context = await _scenarioContextRepository.GetContext(update.Message.From.Id, cancellationToken);
            if (context != null)
            {
                await ProcessScenario(botClient, context, update.Message.From, update.Message, cancellationToken);
                return;
            }
            switch (update.Message.Text)
            {
                case "/start":
                    await StartCommand(botClient, update, cancellationToken);
                    await HelpCommand(botClient, update, cancellationToken);
                    break;
                case "/help":
                    await HelpCommand(botClient, update, cancellationToken);
                    break;
                case string a when a.StartsWith("/add_request"):
                    if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                    {
                        context = new ScenarioContext(ScenarioType.Add_Request);
                        await _scenarioContextRepository.SetContext(update.Message.From.Id, context, cancellationToken);
                        await ProcessScenario(botClient, context, update.Message.From, update.Message, cancellationToken);
                    }
                    break;
                case "/requests":
                    if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                    {
                        ToDoUser user = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, cancellationToken)).Id, cancellationToken);
                        IReadOnlyList<Request> requests = await _scheduleRequestService.GetRequestsAsync(user.Id, cancellationToken);
                        List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                        foreach (Request request in requests)
                        {
                            callbackData.Add(new KeyValuePair<string, string>(request.CreatedAt.ToString("dd MMM yyyy года HH:mm:ss"), ToDoItemCallbackDto.FromString($"showRequest|{request.Id}").ToString()));
                        }
                        if (callbackData.Count == 0)
                        {
                            await botClient.SendMessage(update.Message.Chat, "Вы ещё не подавали заявки", cancellationToken: cancellationToken);
                            break;
                        }
                        await botClient.SendMessage(update.Message.Chat, "Выберите заявку, чтобы посмотреть её статус и описание", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("show||0")), cancellationToken: cancellationToken);
                    }
                    break;
                case "/график":
                    if (await CheckCredentials(update.Message.From, Role.User | Role.Moderator | Role.Administrator, cancellationToken))
                    {

                    }
                    break;
                case "/создать график":
                    break;
                case "/create_template":
                    context = new ScenarioContext(ScenarioType.Add_Template);
                    context.Data["TelegramUserId"] = update.Message.From.Id;
                    context.Data["ChatId"] = update.Message.Chat.Id;
                    await _scenarioContextRepository.SetContext(update.Message.From.Id, context, cancellationToken);
                    await ProcessScenario(botClient, context, update.Message.From, update.Message, cancellationToken);
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.", cancellationToken: cancellationToken);
                    await HelpCommand(botClient, update, cancellationToken);
                    break;
            }
        }

        private async Task HandleCallBack(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ScenarioContext? context = await _scenarioContextRepository.GetContext(update.CallbackQuery.From.Id, ct);
            if (context != null)
            {
                context.Data["Callback"] = update.CallbackQuery.Data;
                context.Data["TelegramUserId"] = update.CallbackQuery.From.Id;
                context.Data["ChatId"] = update.CallbackQuery.Message.Chat.Id;
                await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                return;
            }
            switch (update.CallbackQuery)
            {
                case CallbackQuery a when a.Data.StartsWith("show"):
                    //context = new ScenarioContext(ScenarioType.Add_request);
                    //await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    //await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    ToDoUser user = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(update.CallbackQuery.From.Id, ct)).Id, ct);
                    if(a.Data.StartsWith("showRequest"))
                    {
                        Request request = await _scheduleRequestService.GetRequestAsync(user.Id, ToDoItemCallbackDto.FromString(a.Data).ToDoItemId, ct);
                        InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup();
                        string answer = $"Сообщение:{request.Message}\r\nСтатус:{request.Status.GetDisplayName()}{(request.Processor == null?"":$"\r\nЗаявку обработал{request.Processor.FirstName}{(request.ResolutionComment == null?"":$"Комментарий к заявке:{request.ResolutionComment}")}")}\r\nДата создания заявки:{request.CreatedAt}";
                            keyboardMarkup.AddNewRow(new InlineKeyboardButton[]
                            {
                                new InlineKeyboardButton("❌Удалить",ToDoItemCallbackDto.FromString($"deleteRequest|{request.Id}").ToString())
                            });
                        await botClient.SendMessage(update.CallbackQuery.Message.Chat, answer, replyMarkup: keyboardMarkup, cancellationToken: ct);
                        break;
                    }
                    PagedListCallbackDto dto = PagedListCallbackDto.FromString(a.Data);
                    IReadOnlyList<Request> requests = await _scheduleRequestService.GetRequestsAsync(user.Id, ct);
                    List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                    foreach (Request request in requests)
                    {
                        callbackData.Add(new KeyValuePair<string, string>(request.CreatedAt.ToString("dd MMM yyyy года HH:mm:ss"), ToDoItemCallbackDto.FromString($"showRequest|{request.Id}").ToString()));
                    }
                    await botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "Выберите заявку, чтобы посмотреть её статус и описание", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, dto), cancellationToken: ct);
                    break;
                case CallbackQuery a when a.Data.StartsWith("deleteRequest"):
                    context = new ScenarioContext(ScenarioType.Delete_Request);
                    context.Data.Add("Callback", ToDoItemCallbackDto.FromString(a.Data).ToString());
                    await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    break;
            }
        }

        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, User user, Message msg, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            if (await scenario.HandleMessageAsync(botClient, context, msg, ct) == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(user.Id, ct);
        }

        private IScenario GetScenario(ScenarioType scenarioType)
        {
            foreach (IScenario scenario in _scenarios)
            {
                if (scenario.CanHandle(scenarioType))
                {
                    return scenario;
                }
            }
            throw new ArgumentException("Сценарий не найден");
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
/create_template - процесс создания шаблона графиков;
/создать график - процесс создания графика для пользователя
/график - показывает текущий график;
/add_request - создаёт заявку на смену расписания;
/requests - выводит список заявок;", cancellationToken:ct);
                else if (toDoUser.Role == Role.Moderator)
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:заглушка", cancellationToken: ct);
                else
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/график - показывает текущий график;
/add_request - создаёт заявку на смену расписания;
/requests - выводит список заявок;", cancellationToken: ct);
            }
            else
                await botClient.SendMessage(update.Message.Chat, @"Вот список доступных комманд:/start, /help", cancellationToken: ct);
        }
    }
}