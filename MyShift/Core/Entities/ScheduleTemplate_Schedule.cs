using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Models
{
    public class ScheduleTemplate_Schedule
    {
        public int Id { get; set; }
        public int ScheduleTemplateId { get; set; }
        [ForeignKey("ScheduleTemplateId")]
        public ScheduleTemplate Schedule_Template { get; set; }
        public int FirstScheduleId { get; set; }
        [ForeignKey("FirstScheduleId")]
        public Schedule FirstSchedule{ get; set; }
        //поле отмены продления графика
        public bool Is_Cancelled { get; set; } = false;
        public ScheduleTemplate_Schedule()
        {}
        public ScheduleTemplate_Schedule(ScheduleTemplate scheduleTemplateId, Schedule firstScheduleId)
        {
            Schedule_Template = scheduleTemplateId;
            FirstSchedule = firstScheduleId;
        }
    }
}