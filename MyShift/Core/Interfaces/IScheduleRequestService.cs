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
        Task<Request> ApproveRequestAsync(int requestId, int processorId, string message, CancellationToken ct);
        Task<Request> RejectRequestAsync(int requestId, int processorId, string message, CancellationToken ct);
        Task DeleteRequestAsync(int requestId, CancellationToken ct);
        Task<Request?> GetRequestAsync(int requestId, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct);
        Task SetProcessor(int requestId, int processorId, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct);
        Task<UserSchedule?> GetScheduleAsync(int scheduleId,CancellationToken ct);
        Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct);
        Task<Shift?> GetShiftByIdAsync(int scheduleId, CancellationToken ct);
        Task<Shift?> EditShiftScheduleAsync(int shiftId, TimeSpan startTime, TimeSpan endTime, CancellationToken ct);
        Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct);
        Task<UserSchedule?> DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct);
        Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct);
        Task<DateTime> GenerationDayShifts(UserSchedule schedule, string rulesJson, DateTime firstWorkDay, DateTime lastWorkDay, CancellationToken ct);
        Task<DateTime> GenerationCycleShifts(UserSchedule schedule, string rulesJson, DateTime firstWorkDay, DateTime lastWorkDay, CancellationToken ct);
    }
}