using MyShift.Core.Entities;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    internal interface INotificationRepository
    {
        Task<IReadOnlyList<Notification>> GetScheduledNotifications(DateTime scheduledBefore, CancellationToken ct);
        Task MarkNotified(int notificationId, CancellationToken ct);
        Task<bool> ScheduleNotification(int userId, string type, string text, DateTime scheduledAt, CancellationToken ct);
    }
}