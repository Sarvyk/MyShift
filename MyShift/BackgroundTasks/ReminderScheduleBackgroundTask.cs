using MyShift.Core.Entities;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MyShift.BackgroundTasks
{
    internal class ReminderScheduleBackgroundTask : BackgroundTask
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        public ReminderScheduleBackgroundTask(TimeSpan delay, ITelegramBotClient botClient, IUserRepository userRepository, INotificationService notificationService) : base(delay, nameof(ReminderScheduleBackgroundTask))
        {
            _botClient = botClient;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
            var users = await _userRepository.GetAllUsersAsync(ct);
            foreach (Notification notification in notifications)
            {
                if (notification.Type.StartsWith("Shift_") && DateTime.UtcNow.Hour == 15)
                {
                    foreach (ToDoUser user in users)
                    {
                        if (user.Id == notification.user.Id)
                        {
                            await _botClient.SendMessage(user.TelegramId, notification.Text, cancellationToken: ct);
                            await _notificationService.MarkNotified(notification.id, ct);
                        }
                    }
                }
            }
        }
    }
}