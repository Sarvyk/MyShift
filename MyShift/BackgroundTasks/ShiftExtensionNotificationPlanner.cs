using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.BackgroundTasks
{
    internal class ShiftExtensionNotificationPlanner : BackgroundTask
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly INotificationService _notificationService;

        public ShiftExtensionNotificationPlanner(TimeSpan delay, IScheduleRepository scheduleRepository, INotificationService notificationService) : base(delay, nameof(ShiftExtensionNotificationPlanner))
        {
            _scheduleRepository = scheduleRepository;
            _notificationService = notificationService;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var schedules = await _scheduleRepository.GetActiveSchedulesAsync(ct);
            foreach(var schedule in schedules)
            {
                var shifts = await _scheduleRepository.GetLastShiftsByScheduleIdAsync(schedule.Id, ct);
                DateTime shiftDate = shifts.ShiftDate.Date.AddDays(-1);
                await _notificationService.ScheduleNotification(shifts.UserSchedule.UserId, $"ScheduleExtension_{shifts.UserScheduleId}_{DateOnly.FromDateTime(DateTime.UtcNow)}", "Ваш график был продлён до ", shiftDate, ct);
            }
        }
    }
}