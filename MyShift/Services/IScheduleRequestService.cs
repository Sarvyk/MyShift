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
        Task CreateRequestAsync(int userId, string message);
        Task DeleteRequestAsync(int id, string number);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId);
        Task GetScheduleAsync(ToDoUser toDoUser);
    }
}
