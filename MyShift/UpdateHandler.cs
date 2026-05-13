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
            Role all = Role.User | Role.Moderator | Role.Administrator | Role.SuperAdministrator;
            Role nonUser = Role.Moderator | Role.Administrator | Role.SuperAdministrator;
            Role onlyAdministrator = Role.Administrator | Role.SuperAdministrator;
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
                    if (await CheckCredentials(botClient, update, update.Message.From, all, cancellationToken))
                    {
                        context = new ScenarioContext(ScenarioType.Add_Request);
                        await _scenarioContextRepository.SetContext(update.Message.From.Id, context, cancellationToken);
                        await ProcessScenario(botClient, context, update.Message.From, update.Message, cancellationToken);
                    }
                    break;
                case "/requests":
                    if (await CheckCredentials(botClient, update, update.Message.From, all, cancellationToken))
                    {
                        await GetRequest(botClient, update, cancellationToken);
                    }
                    break;
                case "/schedule":
                    if (await CheckCredentials(botClient, update, update.Message.From, all, cancellationToken))
                    {
                        await GetSchedule(botClient, update, cancellationToken);
                    }
                    break;
                case "/create_schedule":
                    if (await CheckCredentials(botClient, update, update.Message.From, nonUser, cancellationToken))
                    {
                        await CreateSchedule(botClient, update, context, cancellationToken);
                    }
                    break;
                case "/edit_schedule":
                    if (await CheckCredentials(botClient, update, update.Message.From, nonUser, cancellationToken))
                    {
                        await EditSchedule(botClient, update, cancellationToken);
                    }
                    break;
                case "/create_template":
                    if (await CheckCredentials(botClient, update, update.Message.From, onlyAdministrator, cancellationToken))
                    {
                        await CreateTemplate(botClient, update, context, cancellationToken);
                    }
                    break;
                case "/edit_role":
                    if (await CheckCredentials(botClient, update, update.Message.From, nonUser, cancellationToken))
                    {
                        await EditRole(botClient, update, context, cancellationToken);
                    }
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.⚠️", cancellationToken: cancellationToken);
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
                context.Data["CallbackMessage"] = update.CallbackQuery.Message;
                context.Data["TelegramUserId"] = update.CallbackQuery.From.Id;
                context.Data["ChatId"] = update.CallbackQuery.Message.Chat.Id;
                await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                return;
            }
            switch (update.CallbackQuery)
            {
                case CallbackQuery a when a.Data.StartsWith("show"):
                    ToDoUser user = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(update.CallbackQuery.From.Id, ct)).Id, ct);
                    if (a.Data.StartsWith("showRequest"))
                    {
                        if (a.Data.StartsWith("showRequest|"))
                        {
                            Request request = await _scheduleRequestService.GetRequestAsync(user.Id, ToDoItemCallbackDto.FromString(a.Data).ToDoItemId, ct);
                            InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup();
                            string answer = $"Сообщение:{request.Message}\r\nСтатус:{request.Status.GetDisplayName()}{(request.Processor == null ? "" : $"\r\nЗаявку обработал{request.Processor.FirstName}{(request.ResolutionComment == null ? "" : $"Комментарий к заявке:{request.ResolutionComment}")}")}\r\nДата создания заявки:{request.CreatedAt}";
                            keyboardMarkup.AddNewRow(new InlineKeyboardButton[]
                            {
                                new InlineKeyboardButton("❌Удалить",ToDoItemCallbackDto.FromString($"deleteRequest|{request.Id}").ToString())
                            });
                            await botClient.SendMessage(update.CallbackQuery.Message.Chat, answer, replyMarkup: keyboardMarkup, cancellationToken: ct);
                            break;
                        }
                        else if (a.Data.StartsWith("showRequestPagePrev") || a.Data.StartsWith("showRequestPageNext"))
                        {
                            PagedListCallbackDto dto = PagedListCallbackDto.FromString(a.Data);
                            IReadOnlyList<Request> requests = await _scheduleRequestService.GetRequestsAsync(user.Id, ct);
                            List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                            foreach (Request request in requests)
                            {
                                callbackData.Add(GetFormatAnswerRequest(request));
                            }
                            await botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "Выберите заявку📋, чтобы посмотреть🔍 её статус и описание", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, dto), cancellationToken: ct);
                        }
                    }
                    else if (a.Data.StartsWith("showShift"))
                    {
                        if(a.Data.StartsWith("showShift|"))
                        {
                            Shift shift = await _scheduleRequestService.GetShiftByIdAsync(ToDoItemCallbackDto.FromString(a.Data).ToDoItemId, ct);
                            if (shift.ShiftType == ShiftType.off)
                            {
                                await botClient.SendMessage(update.CallbackQuery.Message.Chat, $"На {shift.ShiftDate.ToString("d")} назначен выходной🛌 день.", cancellationToken: ct);
                                return;
                            }
                            string answer = $"{shift.ShiftType.GetDisplayName()} на {shift.ShiftDate.ToString("D")}\r\nВремя с {shift.StartTime.Value.ToString(@"hh\:mm")} до {shift.EndTime.Value.ToString(@"hh\:mm")}{(shift.Status == true?"": "\r\nСтатус:отменена")}";
                            await botClient.SendMessage(update.CallbackQuery.Message.Chat, answer, cancellationToken: ct);
                        }
                        else if(a.Data.StartsWith("showShiftsPagePrev") || a.Data.StartsWith("showShiftsPageNext"))
                        {
                            PagedListCallbackDto dto = PagedListCallbackDto.FromString(a.Data);
                            IReadOnlyList<Shift> shifts = (await _scheduleRequestService.GetActiveScheduleByUserAsync(user.Id, ct)).Shifts.OrderBy(sh => sh.ShiftDate).ToList();
                            List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                            foreach (Shift shift in shifts)
                            {
                                callbackData.Add(GetFormatAnswerShift(shift));
                            }
                            await botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "Выберите смену🗓️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, dto), cancellationToken: ct);
                        }
                    }
                    break;
                case CallbackQuery a when a.Data.StartsWith("deleteRequest"):
                    context = new ScenarioContext(ScenarioType.Delete_Request);
                    context.Data.Add("Callback", ToDoItemCallbackDto.FromString(a.Data).ToString());
                    await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    break;
            }
        }
        private async Task GetRequest(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser user = await _userService.GetUserAsync((await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct)).Id, ct);
            IReadOnlyList<Request> requests = await _scheduleRequestService.GetRequestsAsync(user.Id, ct);
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (Request request in requests)
            {
                callbackData.Add(GetFormatAnswerRequest(request));
            }
            if (callbackData.Count == 0)
            {
                await botClient.SendMessage(update.Message.Chat, "Вы ещё не подавали заявки📄🚫", cancellationToken: ct);
                return;
            }
            await botClient.SendMessage(update.Message.Chat, "Выберите заявку, чтобы посмотреть её статус и описание📋", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showRequestPageNext||0")), cancellationToken: ct);
        }
        private async Task GetSchedule(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            UserSchedule userSchedule = await _scheduleRequestService.GetActiveScheduleByUserAsync((await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct)).Id, ct);
            if (userSchedule == null)
            {
                await botClient.SendMessage(update.Message.Chat, "У вас отсутствует график⚠️", cancellationToken: ct);
                return;
            }
            var callbackData = new List<KeyValuePair<string, string>>();
            foreach (Shift shift in userSchedule.Shifts)
            {
                callbackData.Add(GetFormatAnswerShift(shift));
            }
            await botClient.SendMessage(update.Message.Chat, "Выберите смену🗓️", replyMarkup: PageBuilder.BuildPagedButtons(callbackData, PagedListCallbackDto.FromString("showShiftsPageNext||0")), cancellationToken: ct);
        }
        private async Task EditSchedule(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ScenarioContext context = new ScenarioContext(ScenarioType.Edit_Schedule);
            context.Data.Add("TelegramUserId", update.Message.From.Id);
            context.Data.Add("ChatId", update.Message.Chat.Id);
            await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
            await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
        }
        private async Task CreateSchedule(ITelegramBotClient botClient, Update update, ScenarioContext context, CancellationToken ct)
        {
            context = new ScenarioContext(ScenarioType.Add_Schedule);
            context.Data.Add("TelegramUserId", update.Message.From.Id);
            context.Data.Add("ChatId", update.Message.Chat.Id);
            context.Data.Add("Callback", string.Empty);
            await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
            await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
        }
        private async Task CreateTemplate(ITelegramBotClient botClient, Update update, ScenarioContext context, CancellationToken ct)
        {
            context = new ScenarioContext(ScenarioType.Add_Template);
            context.Data.Add("TelegramUserId", update.Message.From.Id);
            context.Data.Add("ChatId", update.Message.Chat.Id);
            await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
            await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
        }
        private async Task EditRole(ITelegramBotClient botClient, Update update, ScenarioContext context, CancellationToken ct)
        {
            context = new ScenarioContext(ScenarioType.Edit_Role);
            context.Data.Add("TelegramUserId", update.Message.From.Id);
            context.Data.Add("ChatId", update.Message.Chat.Id);
            await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
            await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
        }
        private KeyValuePair<string,string> GetFormatAnswerRequest(Request request) => new KeyValuePair<string, string>(request.CreatedAt.ToString("dd MMM yyyy года HH:mm:ss"), ToDoItemCallbackDto.FromString($"showRequest|{request.Id}").ToString());
        private KeyValuePair<string, string> GetFormatAnswerShift(Shift shift) => new KeyValuePair<string, string>(shift.ShiftDate.ToString("d"), ToDoItemCallbackDto.FromString($"showShift|{shift.Id}").ToString());
        private KeyValuePair<string, string> GetFormatAnswerUser(ToDoUser user) => new KeyValuePair<string, string>($"{user.Id}){user.FirstName} {user.LastName}", ToDoItemCallbackDto.FromString($"showUser|{user.Id}").ToString());
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
            throw new ArgumentException("Сценарий не найден⚠️");
        }

        private async Task StartCommand(ITelegramBotClient botClient,Update update, CancellationToken ct)
        {
            ToDoUser? toDoUser = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            if (toDoUser != null) 
            {
                await botClient.SendMessage(update.Message.Chat, "Бот уже запущен!⚠️", cancellationToken: ct);
            }
            else
            {
                toDoUser = await _userService.RegisterUserAsync(update.Message.Chat.Id, update.Message.From, ct);
                await botClient.SendMessage(update.Message.Chat, $"{toDoUser.FirstName}, добро пожаловать в бот👋🏻🤖 \"Мой график\"📋!", cancellationToken: ct);
            }
            await MarkupManager.SetCommand(botClient, toDoUser.Role, update.Message.Chat, ct);
        }
        private async Task<bool> CheckCredentials(ITelegramBotClient botClient, Update update, User user, Role roles, CancellationToken ct)
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
                    await botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.⚠️", cancellationToken: ct);
                    return false;
                }
            }
            else
            {
                await botClient.SendMessage(update.Message.Chat, $"Такой команды не существует.⚠️", cancellationToken: ct);
                return false;
            }
        }
        private async Task HelpCommand(ITelegramBotClient botClient,Update update, CancellationToken ct)
        {
            ToDoUser? toDoUser = await _userService.GetUserByTelegramIdAsync(update.Message.From.Id, ct);
            if (toDoUser != null)
            {
                if(toDoUser.Role == Role.SuperAdministrator)
                {
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/help - показать помощь;
/create_template - процесс создания шаблона графиков;✍️
/create_schedule - процесс создания графика для пользователя;✍️
/edit_schedule - редактирование графиков;🔄
/schedule - показывает текущий график;📋
/add_request - создаёт заявку на смену расписания;✍️
/edit_role - смена ролей;🔄
/requests - выводит список заявок;📋", cancellationToken: ct);
                }
                else if (toDoUser.Role == Role.Administrator)
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/help - показать помощь;
/create_template - процесс создания шаблона графиков;✍️
/create_schedule - процесс создания графика для пользователя;✍️
/edit_schedule - редактирование графиков;🔄
/schedule - показывает текущий график;📋
/add_request - создаёт заявку на смену расписания;✍️
/edit_role - смена ролей;🔄
/requests - выводит список заявок;📋", cancellationToken:ct);
                else if (toDoUser.Role == Role.Moderator)
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:заглушка", cancellationToken: ct);
                else
                    await botClient.SendMessage(update.Message.Chat, @"Список команд:
/help - показать помощь;
/schedule - показывает текущий график📋;
/add_request - создаёт заявку на смену расписания; ✍️
/requests - выводит список заявок;📋", cancellationToken: ct);
            }
            else
                await botClient.SendMessage(update.Message.Chat, @"Вот список доступных комманд:/start▶️, /help🆘", cancellationToken: ct);
        }
    }
}