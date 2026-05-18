using Microsoft.EntityFrameworkCore;
using MyShift.Core.Data;
using MyShift.Core.Entities;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift
{
    internal class NotificationService : INotificationService
    {
        private readonly SqLiteDbContext _context;
        public NotificationService(SqLiteDbContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            return await _context.Notifications
                .Include(n => n.user)
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);
        }

        public async Task MarkNotified(int notificationId, CancellationToken ct)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.id == notificationId, ct);
            if (notification == null)
                throw new Exception("Такого уведомления не существует");
            notification.IsNotified = true;
            notification.NotifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ScheduleNotification(int userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (await _context.Notifications.AnyAsync(n => n.user.Id == userId && n.Type == type, ct))
                return false;
            Notification notification = new Notification()
            {
                user = user,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt
            };
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}