using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    public class CycleShift
    {
        public int Id { get; set; }
        //public string Name { get; set; }
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]//ссылка на шаблон
        public ScheduleTemplate Template { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }//день, ночь или выходной
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}