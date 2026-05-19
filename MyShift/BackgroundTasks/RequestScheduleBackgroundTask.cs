
using MyShift.Core.Entities;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.DTO;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.BackgroundTasks
{
    internal class RequestScheduleBackgroundTask : BackgroundTask
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        public RequestScheduleBackgroundTask(TimeSpan delay, ITelegramBotClient botClient, IUserRepository userRepository, INotificationService notificationService) : base(delay, nameof(RequestScheduleBackgroundTask))
        {
            _botClient = botClient;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
            var Staff = await _userRepository.GetStaff(ct);
            foreach (Notification notification in notifications)
            {
                if (notification.Type.StartsWith("Request"))
                {
                    int requestId = Int32.Parse(notification.Type.Split("_")[1]);
                    foreach (ToDoUser user in Staff)
                    {
                        await _botClient.SendMessage(user.TelegramId, $"Запрос от пользователя {notification.user.Id}){notification.user.FirstName} {notification.user.LastName}\r\nСообщение:{notification.Text}",
                            replyMarkup: new InlineKeyboardMarkup()
                            .AddButton("Обработать заявку", ToDoItemCallbackDto.FromString($"TakeRequest|{requestId}").ToString())
                            .AddButton("Отказать", ToDoItemCallbackDto.FromString($"CancelRequest|{requestId}").ToString()),
                            cancellationToken: ct);
                        await _notificationService.MarkNotified(notification.id, ct);
                    }
                }
            }
        }
    }
}