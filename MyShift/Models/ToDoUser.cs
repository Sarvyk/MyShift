using MyShift.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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
        [InverseProperty("Creator")]
        public List<Request> CreatedRequests { get; set; } = new(); // Заявки пользователя
        [InverseProperty("Processor")]
        public List<Request> ProcessedRequests { get; set; } = new(); // Обработанные заявки
        [InverseProperty("AssignedBy")]
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