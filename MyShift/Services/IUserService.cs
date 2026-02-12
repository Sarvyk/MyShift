using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Services
{
    internal interface IUserService
    {
        Task<ToDoUser?> GetUserAsync(int id, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct);
        Task<ToDoUser?> RegisterUserAsync(long chatId,User userData, CancellationToken ct);
        Task<IReadOnlyList<ToDoUser>> GetAllUsers(CancellationToken ct);
    }
}
