using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Models
{
    public class Schedule_Template
    {//Данная таблица будет содержать шаблон графика, к которому будут привязаны другие графики из Schedule.cs
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime {  get; set; }
        public TimeSpan EndTime { get; set; }
        [Column(TypeName = "bit(7)")]
        public string DaysOfWeekBits { get; set; }
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public ToDoUser CreatorBy {  get; set; }
        public List<Schedule> Schedules { get; set; } = [];
    }
}