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
        Task RegisterUserAsync(ToDoUser user);
        Task<ToDoUser> GetUserByIdAsync(int id);
        Task<ToDoUser> GetUserByTelegramIdAsync(long id);
    }
}
