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
        //void RegisterUser(ToDoUser user);
        ToDoUser? GetUser(int id);
        ToDoUser? GetUserByTelegramId(long telegramId);
        ToDoUser RegisterUser(User userData);
    }
}
