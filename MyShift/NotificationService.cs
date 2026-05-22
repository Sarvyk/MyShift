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
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public NotificationService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Notifications
                .Include(n => n.user)
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);
        }

        public async Task MarkNotified(int notificationId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            var notification = await context.Notifications.FirstOrDefaultAsync(n => n.id == notificationId, ct);
            if (notification == null)
                throw new Exception("Такого уведомления не существует");
            notification.IsNotified = true;
            notification.NotifiedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        public async Task<bool> ScheduleNotification(int userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (await context.Notifications.AnyAsync(n => n.user.Id == userId && n.Type == type, ct))
                return false;
            Notification notification = new Notification()
            {
                user = user,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt
            };
            await context.AddAsync(notification);
            await context.SaveChangesAsync(ct);
            return true;
        }
        public async Task<Notification?> GetNotificationByUserIdAndType(int userId, string type, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Notifications.FirstOrDefaultAsync(u => u.user.Id == userId && u.Type.StartsWith(type), ct);
        }
    }
}