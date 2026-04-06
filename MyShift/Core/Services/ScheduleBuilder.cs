using MyShift.Core.Interfaces;
using MyShift.Core.Models;

namespace MyShift.Core.Services
{
    internal class ScheduleBuilder : IScheduleBuilder
    {
        private readonly ScheduleTemplate _template;//не знаю, стоит ли это разделять. Чисто технически, этот класс работает создаёт шаблоны графиков и сами графики. Поидее смысла особого нет в разделении
        private readonly Schedule _schedule;
        private readonly List<CycleShift> _cycleList;
        public ScheduleBuilder(ToDoUser creator)
        {
            _template = new ScheduleTemplate(creator);
            _schedule = new Schedule(creator);
            _cycleList = new List<CycleShift>();
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