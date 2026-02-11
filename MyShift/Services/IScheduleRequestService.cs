using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Services
{
    internal interface IScheduleRequestService
    {
        Task CreateRequestAsync(int userId, string message, CancellationToken ct);
        Task CreateScheduleTemplateAsync(Schedule_Template sch_template, CancellationToken ct);
        Task DeleteRequestAsync(int id, string number, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct);
        Task GetScheduleAsync(ToDoUser toDoUser,CancellationToken ct);
    }
}
