using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    internal interface IScheduleBuilder
    {
        public void AddNameTemplate(string name);
        public void AddStartTimeTemplate(TimeSpan timeSpan);
        public void AddEndTimeTemplate(TimeSpan timeSpan);
        public void AddDaysOfWeekTemplate(string bitWeek);
        public void AddUserSchedule(ToDoUser user);
        public void AddDateSchedule(DateTime date);
        public Schedule GetSchedule();
        public ScheduleTemplate GetTemplate();
    }
}