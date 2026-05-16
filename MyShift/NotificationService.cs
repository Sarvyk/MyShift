using MyShift.Core.Entities;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift
{
    internal class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            return await _notificationRepository.GetScheduledNotifications(scheduledBefore, ct);
        }

        public async Task MarkNotified(int notificationId, CancellationToken ct)
        {
            await _notificationRepository.MarkNotified(notificationId, ct);
        }

        public async Task<bool> ScheduleNotification(Request request, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            return await _notificationRepository.ScheduleNotification(request, type, text, scheduledAt,ct);
        }
    }
}