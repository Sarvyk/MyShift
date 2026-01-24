using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace MyShift.Models
{//График пользователя.
    public class Schedule
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }  // Конкретный день

        // Связь с пользователем
        public int UserId { get; set; }
        public ToDoUser User { get; set; }

        // Кто назначил (админ/модератор)
        [ForeignKey("AssignedBy")]
        public int AssignedById { get; set; }
        public ToDoUser AssignedBy { get; set; }

        // Заявка на изменение (если есть)
        public int? ChangeRequestId { get; set; }
        [ForeignKey("ChangeRequestId")]
        public Request? ChangeRequest { get; set; }
    }
}