using MyShift.Core.Extensions;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace MyShift.BackgroundTasks
{
    internal class RemindersCollectionNotificationPlanner : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IScheduleRepository _scheduleRepository;

        public RemindersCollectionNotificationPlanner(TimeSpan delay, INotificationService notificationService, IScheduleRepository scheduleRepository) : base(delay, nameof(RemindersCollectionNotificationPlanner))
        {
            _notificationService = notificationService;
            _scheduleRepository = scheduleRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var shifts = await _scheduleRepository.GetAllShiftsForTomorrowAsync(ct);
            foreach(Shift shift in shifts)
            {
                await _notificationService.ScheduleNotification(shift.UserSchedule.User.Id, $"Shift_{shift.Id}_{DateOnly.FromDateTime(DateTime.UtcNow)}", 
                    $"У вас на завтра ({shift.ShiftDate.Date.ToString("D")}) назначена {shift.ShiftType.GetDisplayName()} с {shift.StartTime} до {shift.EndTime}", DateTime.UtcNow, ct);
            }
        }
    }
}