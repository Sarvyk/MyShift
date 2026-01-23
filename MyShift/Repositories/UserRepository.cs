using Microsoft.EntityFrameworkCore;
using MyShift.Data;
using MyShift.Models;
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
        public async Task RegisterUserAsync(ToDoUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<ToDoUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ToDoUser> GetUserByTelegramIdAsync(long telegramId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
        }
    }
}
