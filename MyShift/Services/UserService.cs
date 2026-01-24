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
        public ToDoUser? GetUser(int id)
        {
            return _userRepository.GetUserByIdAsync(id).Result;
        }

        public ToDoUser? GetUserByTelegramId(long telegramId)
        {
            return _userRepository.GetUserByTelegramIdAsync(telegramId).Result;
        }

        public ToDoUser? RegisterUser(User userData)
        {
            ToDoUser toDoUser = new ToDoUser(userData.Id, userData.Username, userData.FirstName, userData.LastName);
            Console.WriteLine(toDoUser.Role);
            _userRepository.RegisterUserAsync(toDoUser);
            return toDoUser;
        }
    }
}