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
        public int UserId { get; set; }// Чей график
        [ForeignKey("UserId")]
        public ToDoUser User { get; set; }
        public int AssignedById { get; set; }// Кто назначил (админ/модератор)
        [ForeignKey("AssignedById")]
        public ToDoUser AssignedBy { get; set; }
        public int? ParentId { get; set; }//ссылка на родительские записи
        [ForeignKey("ParentId")]
        public Schedule Parent {  get; set; }
        public bool Is_Cancelled { get; set; } = false;//Это свойство для отмены смены в конкретный день.
        public Schedule() { }
        public Schedule(ToDoUser assignedBy)
        {
            AssignedBy = assignedBy;
            Date = DateTime.Now;
        }
        public Schedule(DateTime date, int userId, int assignedById, int parentId, bool addParent = false)
        {
            DateTime dateResult = new DateTime(date.Year, date.Month, date.Day, 0,0,0);
            Date = dateResult;
            UserId = userId;
            AssignedById = assignedById;
            if(addParent)
                ParentId = parentId;
        }
        public Schedule Clone(DateTime date)
        {
            return new Schedule(date, this.UserId, this.AssignedById, this.Id, true);
        }
    }
}