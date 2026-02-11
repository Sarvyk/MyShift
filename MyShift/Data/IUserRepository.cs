using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Data
{
    public interface IUserRepository
    {
        Task RegisterUserAsync(ToDoUser user, CancellationToken ct);
        Task<ToDoUser?> GetUserByIdAsync(int id, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramIdAsync(long id, CancellationToken ct);
    }
}
