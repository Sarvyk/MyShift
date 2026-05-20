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
        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task<IReadOnlyList<ToDoUser>> GetAllUsersAsync(CancellationToken ct)
        {
            return _userRepository.GetAllUsersAsync(ct);
        }
        /// <summary>
        /// Получить всех пользователей с ролями выше оператора
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<ToDoUser>> GetStaffAsync(CancellationToken ct)
        {
            return await _userRepository.GetStaffAsync(ct);
        }
        /// <summary>
        /// Получить пользователя по id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ToDoUser?> GetUserAsync(int id, CancellationToken ct)
        {
            return await _userRepository.GetUserByIdAsync(id,ct);
        }
        /// <summary>
        /// Получить пользователя по TelegramID
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ToDoUser?> GetUserByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            return await _userRepository.GetUserByTelegramIdAsync(telegramId, ct);
        }
        /// <summary>
        /// Зарегистрировать пользователя
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="userData"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ToDoUser?> RegisterUserAsync(long chatId, User userData, CancellationToken ct)
        {
            ToDoUser toDoUser = new ToDoUser(chatId, userData.Id, userData.Username, userData.FirstName, userData.LastName);
            await _userRepository.RegisterUserAsync(toDoUser, ct);
            return toDoUser;
        }
        /// <summary>
        /// Установить пользователю роль
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task SetRoleAsync(int userId, Role role, CancellationToken ct)
        {
            await _userRepository.SetRoleAsync(userId, role, ct);
        }
    }
}