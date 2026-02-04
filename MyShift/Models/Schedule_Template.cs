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
        public DateTime StartTime {  get; set; }
        public DateTime EndTime { get; set; }
        [Column(TypeName = "bit(7)")]
        public string DaysOfWeekBits { get; set; }
        public List<Schedule> Schedules { get; set; } = new();
    }
}