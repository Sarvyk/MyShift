using MyShift.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Models
{
    public class ToDoUser
    {
        public int Id { get; set; }
        public long TelegramId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public Role Role { get; set; }
        public List<Request> CreatedRequests { get; set; } = new(); // Заявки пользователя
        public List<Request> ProcessedRequests { get; set; } = new(); // Обработанные заявки
        public List<Schedule> Schedules { get; set; } = new(); // Графики пользователя
        public ToDoUser(long telegramId, string? userName, string? firstName, string? lastName)
        {
            TelegramId = telegramId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            RegisteredAt = DateTime.Now;
        }
        public ToDoUser() { }
    }
}