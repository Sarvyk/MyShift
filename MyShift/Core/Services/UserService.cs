using MyShift.Core.Enums;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MyShift.Core.Services
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository repository)
        {
            _userRepository = repository;
        }

        public Task<IReadOnlyList<ToDoUser>> GetAllUsers(CancellationToken ct)
        {
            return _userRepository.GetAllUsersAsync(ct);
        }

        public async Task<ToDoUser?> GetUserAsync(int id, CancellationToken ct)
        {
            return await _userRepository.GetUserByIdAsync(id,ct);
        }

        public async Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            return await _userRepository.GetUserByTelegramIdAsync(telegramId, ct);
        }

        public async Task<ToDoUser?> RegisterUserAsync(long chatId, User userData, CancellationToken ct)
        {
            ToDoUser toDoUser = new ToDoUser(chatId, userData.Id, userData.Username, userData.FirstName, userData.LastName);
            await _userRepository.RegisterUserAsync(toDoUser, ct);
            return toDoUser;
        }

        public async Task SetRole(int userId, Role role, CancellationToken ct)
        {
            await _userRepository.SetRole(userId, role, ct);
        }
    }
}