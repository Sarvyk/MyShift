using MyShift.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.BackgroundTasks
{
    internal class CollectRequestsBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IRequestRepository _requestRepository;
        public CollectRequestsBackgroundTask(TimeSpan delay, INotificationService notificationService, IRequestRepository requestRepository) : base(delay, nameof(CollectRequestsBackgroundTask))
        {
            _notificationService = notificationService;
            _requestRepository = requestRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var requests = await _requestRepository.GetActiveRequestsAsync(ct);
            foreach(var request in requests)
            {
                await _notificationService.ScheduleNotification(request.CreatorId, $"Request_{request.Id}_{DateOnly.FromDateTime(DateTime.UtcNow)}", request.Message, DateTime.UtcNow, ct);
            }
        }
    }
}