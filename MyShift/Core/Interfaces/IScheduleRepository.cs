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
        Task<Schedule?> InsertScheduleAsync(Schedule schedule, CancellationToken ct);
        Task InsertScheduleRangeAsync(List<Schedule> schedules, CancellationToken ct);
        Task InsertSchedule_Template(ScheduleTemplate_Schedule templ_schedule, CancellationToken ct);
        Task<Schedule?> GetSchedule(int scheduleId, CancellationToken ct);
        Task<ScheduleTemplate?> GetScheduleTemplateAsync(int templateId, CancellationToken ct);
        Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplates();
        Task InsertTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct);
        Task EditShiftScheduleAsync( CancellationToken ct);
        Task DeleteScheduleAsync( CancellationToken ct);
    }
}