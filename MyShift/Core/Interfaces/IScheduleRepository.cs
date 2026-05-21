using Microsoft.EntityFrameworkCore;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Core.Interfaces
{
    public interface IScheduleRepository
    {
        Task<UserSchedule?> InsertScheduleAsync(UserSchedule schedule, CancellationToken ct);
        Task InstertShiftsAsync(List<Shift> shifts, CancellationToken ct);
        Task<UserSchedule?> GetScheduleAsync(int scheduleId, CancellationToken ct);
        Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct);
        Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken ct);
        Task<IReadOnlyList<Shift?>> GetAllShiftsForTomorrowAsync(CancellationToken ct);
        Task<Shift?> GetLastShiftsByScheduleIdAsync(int scheduleId, CancellationToken ct);
        Task<ScheduleTemplate?> GetScheduleTemplateAsync(int templateId, CancellationToken ct);
        Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct);
        Task InsertTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct);
        Task<Shift?> EditShiftScheduleAsync(int shiftId, TimeSpan startTime, TimeSpan endTime, CancellationToken ct);
        Task DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct);
        Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct);
        Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct);
    }
}