using MyShift.Enums;
using MyShift.Helpers;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Services
{
    internal class ScheduleBuilder : IScheduleBuilder
    {
        private ScheduleTemplate _template;//не знаю, стоит ли это разделять. Чисто технически, этот класс работает создаёт шаблоны графиков и сами графики. Поидее смысла особого нет в разделении
        private  Schedule _schedule;
        public ScheduleBuilder(ToDoUser creator)
        {
            _template = new ScheduleTemplate(creator);
            _schedule = new Schedule(creator);
        }
        public void AddNameTemplate(string name) => _template.Name = name;
        public void AddStartTimeTemplate(TimeSpan timeSpan) => _template.StartTime = timeSpan;
        public void AddEndTimeTemplate(TimeSpan timeSpan) => _template.EndTime = timeSpan;
        public void AddDaysOfWeekTemplate(string bitWeek) => _template.DaysOfWeekBits = bitWeek;
        public void AddUserSchedule(ToDoUser user) => _schedule.User = user;
        public void AddDateSchedule(DateTime date) => _schedule.Date = date;
        public Schedule GetSchedule()
        {
            return _schedule;
        }

        public ScheduleTemplate GetTemplate()
        {
            return _template;
        }

    }
}