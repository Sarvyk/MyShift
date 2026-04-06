using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    internal interface IScheduleRequestService
    {
        Task InsertRequestAsync(int userId, string message, CancellationToken ct);
        Task InsertScheduleAsync(Schedule schedules, ScheduleTemplate template,CancellationToken ct);
        Task InsertScheduleTemplateAsync(ScheduleTemplate sch_template, CancellationToken ct);
        Task<ScheduleTemplate?> GetTemplateAsync(int templateId, CancellationToken ct);
        Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplates();
        Task DeleteRequestAsync(int requestId, CancellationToken ct);
        Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct);
        Task GetScheduleAsync(ToDoUser toDoUser,CancellationToken ct);
    }
}