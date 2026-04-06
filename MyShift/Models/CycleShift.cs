using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Models
{
    public class CycleShift
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public ScheduleTemplate Template { get; set; }
        public int Position { get; set; }
        public int Type { get; set; }//день, ночь или выходной
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}