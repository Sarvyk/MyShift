using Microsoft.EntityFrameworkCore;
using MyShift.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShift.Core.Models
{
    [Index(nameof(Id), IsUnique = true, Name = "uq_ToDoUser_Id")]
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
        public List<Request> CreatedRequests { get; set; } = new();
        [InverseProperty("Processor")]
        public List<Request> ProcessedRequests { get; set; } = new();
        public ToDoUser(long chatId,long telegramId, string? userName, string? firstName, string? lastName)
        {
            ChatId = chatId;
            TelegramId = telegramId;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            RegisteredAt = DateTime.UtcNow;
            Role = Role.None;
        }
        public ToDoUser() { }
    }
}