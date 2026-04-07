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
        public int UserId { get; set; }// Чей график
        [ForeignKey("UserId")]
        public ToDoUser User { get; set; }
        public int AssignedById { get; set; }// Кто назначил (админ/модератор)
        [ForeignKey("AssignedById")]
        public ToDoUser AssignedBy { get; set; }
        public int TemplateId { get;set; }
        public ScheduleTemplate Template { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public UserSchedule() { }
        public UserSchedule(ToDoUser assignedBy)
        {
            AssignedBy = assignedBy;
            //Date = DateTime.Now;
        }
        public UserSchedule(DateTime date, int userId, int assignedById, int parentId, bool addParent = false)
        {
            DateTime dateResult = new DateTime(date.Year, date.Month, date.Day, 0,0,0);
        }
        public UserSchedule Clone(DateTime date)
        {
            return new UserSchedule(date, this.UserId, this.AssignedById, this.Id, true);
        }
    }
}