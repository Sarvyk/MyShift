using Microsoft.EntityFrameworkCore;
using MyShift.Core.Data;
using MyShift.Core.Enums;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly SqLiteDbContext _context;
        public UserRepository(SqLiteDbContext context)
        {
            _context = context;
        }
        public async Task<ToDoUser> RegisterUserAsync(ToDoUser user, CancellationToken ct)
        {
            await _context.Users.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
            return user;
        }

        public async Task<ToDoUser?> GetUserByIdAsync(int id, CancellationToken ct)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId,ct);
        }

        public async Task<IReadOnlyList<ToDoUser>> GetAllUsersAsync(CancellationToken ct)
        {
            return await _context.Users.ToListAsync(cancellationToken:ct);
        }

        public async Task SetRole(int userId, Role role, CancellationToken ct)
        {
            ToDoUser user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
            user.Role = role;
            await _context.SaveChangesAsync(ct);
        }
    }
}
