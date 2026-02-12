using MyShift.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShift.Models
{
    public class ToDoUser
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
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
        [InverseProperty("User")]
        public List<Schedule> UsersSchedules {get;set;} = new();//Все графики конкретно этого пользователя
        [InverseProperty("AssignedBy")]
        public List<Schedule> Schedules { get; set; } = new(); // Графики пользователя, которые назначал данные пользователь(модератор, админ)
        [InverseProperty("CreatorBy")]
        public List<ScheduleTemplate> Schedule_Templates { get; set; } = new();
        public ToDoUser(long chatId,long telegramId, string? userName, string? firstName, string? lastName)
        {
            ChatId = chatId;
            TelegramId = telegramId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            RegisteredAt = DateTime.UtcNow;
            Role = Role.Administrator;
        }
        public ToDoUser() { }
    }
}