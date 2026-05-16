
using MyShift.Core.Entities;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using MyShift.DTO;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyShift.BackgroundTasks
{
    internal class RequestsDeliveryBackgroundTask : BackgroundTask
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        public RequestsDeliveryBackgroundTask(TimeSpan delay, ITelegramBotClient botClient, IUserRepository userRepository, INotificationService notificationService) : base(delay, nameof(RequestsDeliveryBackgroundTask))
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
                foreach (ToDoUser user in Staff)
                {
                    await _botClient.SendMessage(user.TelegramId, $"Запрос от пользователя {notification.request.CreatorId}){notification.request.Creator.FirstName} {notification.request.Creator.LastName}\r\nСообщение:{notification.Text}",
                        replyMarkup: new InlineKeyboardMarkup().AddButton("Обработать заявку", ToDoItemCallbackDto.FromString($"TakeRequest|{notification.request.Id}").ToString()),
                        cancellationToken: ct);
                    await _notificationService.MarkNotified(notification.id, ct);
                }
            }
        }
    }
}