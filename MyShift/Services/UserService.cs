using MyShift.Data;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Services
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository repository)
        {
            _userRepository = repository;
        }
        public async Task<ToDoUser?> GetUserAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId)
        {
            return await _userRepository.GetUserByTelegramIdAsync(telegramId);
        }

        public async Task<ToDoUser?> RegisterUserAsync(long chatId, User userData)
        {
            ToDoUser toDoUser = new ToDoUser(chatId, userData.Id, userData.Username, userData.FirstName, userData.LastName);
            await _userRepository.RegisterUserAsync(toDoUser);
            return toDoUser;
        }
    }
}