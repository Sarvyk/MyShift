using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Models
{
    public class ScheduleTemplate
    {//Данная таблица будет содержать шаблон графика, к которому будут привязаны другие графики из Schedule.cs
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }//тут будет храниться тип графика. Фиксированный или циклический.
        public TimeSpan? StartTime {  get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? DaysOfWeekBits { get; set; }
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public ToDoUser CreatorBy {  get; set; }
        [InverseProperty("User")]
        public List<CycleShift> CycleShift { get; set; } = new();//Все графики конкретно этого пользователя
        public ScheduleTemplate() { }
        public ScheduleTemplate(ToDoUser creator)
        {  
            CreatorBy = creator;
        }
    }
}