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

namespace MyShift.Infrastructure.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public UserRepository(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<ToDoUser> RegisterUserAsync(ToDoUser user, CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            await context.Users.AddAsync(user, ct);
            await context.SaveChangesAsync(ct);
            return user;
        }

        public async Task<ToDoUser?> GetUserByIdAsync(int id, CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId,ct);
        }

        public async Task<IReadOnlyList<ToDoUser>> GetAllUsersAsync(CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            return await context.Users.ToListAsync(ct);
        }

        public async Task SetRoleAsync(int userId, Role role, CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            ToDoUser user = await context.Users.FirstOrDefaultAsync(user => user.Id == userId, ct);
            user.Role = role;
            await context.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<ToDoUser>> GetStaffAsync(CancellationToken ct)
        {
            await using var context = _dbFactory.CreateDbContext();
            Role roles = Role.Operator | Role.Administrator | Role.SuperAdministrator;
            return await context.Users.Where(user => (user.Role & roles) != 0).ToListAsync(ct);
        }
    }
}