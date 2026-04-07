using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    public class ScheduleTemplate
    {//Данная таблица будет содержать шаблон графика, к которому будут привязаны другие графики из Schedule.cs
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }//тут будет храниться тип графика. Фиксированный или циклический.
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public ToDoUser CreatorBy {  get; set; }
        public string RulesJson { get; set; }
        public ScheduleTemplate() { }
        public ScheduleTemplate(ToDoUser creator)
        {  
            CreatorBy = creator;
        }
    }
}