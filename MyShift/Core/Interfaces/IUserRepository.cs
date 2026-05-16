using MyShift.Core.Enums;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<ToDoUser?> RegisterUserAsync(ToDoUser user, CancellationToken ct);
        Task<ToDoUser?> GetUserByIdAsync(int userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramIdAsync(long userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoUser>> GetAllUsersAsync(CancellationToken ct);
        Task SetRole(int userId, Role role, CancellationToken ct);
        Task<IReadOnlyList<ToDoUser>> GetStaff(CancellationToken ct);
    }
}
