using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;

namespace MyShift.Core.Models
{//График пользователя.
    public class UserSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ToDoUser User { get; set; }
        public int AssignedById { get; set; }// Кто назначил (админ/модератор)
        [ForeignKey("AssignedById")]
        public ToDoUser AssignedBy { get; set; }

        public int TemplateId { get; set; }
        public ScheduleTemplate Template { get; set; }
        public List<Shift> Shifts { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public UserSchedule() { }
        public UserSchedule(int userId, int assignedBy, int templateId)
        {
            UserId = userId;
            AssignedById = assignedBy;
            TemplateId = templateId;
            IsActive = true;
        }
    }
}