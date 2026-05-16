using MyShift.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.BackgroundTasks
{
    internal class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IRequestRepository _requestRepository;
        public TodayBackgroundTask(TimeSpan delay, INotificationService notificationService, IRequestRepository requestRepository) : base(delay, nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService;
            _requestRepository = requestRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var requests = await _requestRepository.GetActiveRequestsAsync(ct);
            foreach(var request in requests)
            {
                await _notificationService.ScheduleNotification(request, $"Request_{DateOnly.FromDateTime(DateTime.UtcNow)}", request.Message, DateTime.UtcNow, ct);
            }
        }
    }
}