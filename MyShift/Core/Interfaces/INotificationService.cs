using MyShift.Core.Entities;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    internal interface INotificationService
    {
        //Создает нотификацию. Если запись с userId и type уже есть, то вернуть false и не добавлять запись, иначе вернуть true
        Task<bool> ScheduleNotification(int userId, string type, string text, DateTime scheduledAt, CancellationToken ct);

        //Возвращает нотификации, у которых IsNotified = false && ScheduledAt <= scheduledBefore
        Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);

        Task MarkNotified(int notificationId, CancellationToken ct);
    }
}