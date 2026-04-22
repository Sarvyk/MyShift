using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    public interface IScheduleRequestService
    {
        Task InsertRequestAsync(int userId, string message, CancellationToken ct);
        Task InsertScheduleAsync(UserSchedule schedules, ScheduleTemplate template,CancellationToken ct);
        Task InsertScheduleTemplateAsync(ScheduleTemplate sch_template, CancellationToken ct);
        Task<ScheduleTemplate?> GetTemplateAsync(int templateId, CancellationToken ct);
        Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct);
        Task DeleteRequestAsync(int requestId, CancellationToken ct);
        Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct);
        Task<UserSchedule?> GetScheduleAsync(int scheduleId,CancellationToken ct);
        Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct);
        Task<Shift?> GetShiftByIdAsync(int scheduleId, CancellationToken ct);
    }
}