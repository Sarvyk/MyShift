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
        // Связь с пользователем
        public DateTime Date { get; set; }  // Конкретный день
        public DateTime StartTime { get; set; } //Начало рабочего дня
        public DateTime EndTime { get; set; }
        // Чей график
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ToDoUser User { get; set; }
        // Кто назначил (админ/модератор)
        public int AssignedById { get; set; }
        [ForeignKey("AssignedById")]
        public ToDoUser AssignedBy { get; set; }
        //статус смены.
        public bool Is_Cancelled { get; set; } = false;
    }
}