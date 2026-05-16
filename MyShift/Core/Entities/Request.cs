using MyShift.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    public class Request
    {//Тут заявка
        public int Id { get; set; }
        public string Message { get; set; }
        public RequestStatus Status { get; set; } // Enum: Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ResolutionComment { get; set; } // Комментарий при обработке

        // Связь с создателем
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public ToDoUser Creator { get; set; }

        // Связь с обработавшим (админ/модератор)
        public int? ProcessorId { get; set; }  // Nullable - может быть не обработана
        [ForeignKey("ProcessorId")]
        public ToDoUser? Processor { get; set; }
        public Request() { }
        public Request(int userId, string message)
        {
            CreatorId = userId;
            Message = message;
            Status = RequestStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }
        public string GetStatus()
        {
            switch (Status)
            {
                case RequestStatus.Pending:
                    return "";
                case RequestStatus.Approved:
                    return "";
                case RequestStatus.Rejected:
                    return "";
                default:
                    return "нет статуса";
            }
        }
    }
}
