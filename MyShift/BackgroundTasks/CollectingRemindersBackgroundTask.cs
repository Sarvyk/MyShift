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
    internal class CollectingRemindersBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IScheduleRepository _scheduleRepository;

        public CollectingRemindersBackgroundTask(TimeSpan delay, INotificationService notificationService, IScheduleRepository scheduleRepository) : base(delay, nameof(CollectingRemindersBackgroundTask))
        {
            _notificationService = notificationService;
            _scheduleRepository = scheduleRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var shifts = await _scheduleRepository.GetAllShiftsForTomorrow(ct);
            foreach(Shift shift in shifts)
            {
                //await _notificationService.ScheduleNotification(shift, $"Request_{DateOnly.FromDateTime(DateTime.UtcNow)}", request.Message, DateTime.UtcNow, ct);
            }
        }
    }
}