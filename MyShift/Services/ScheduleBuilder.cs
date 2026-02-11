using MyShift.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyShift.Services
{
    internal class ScheduleBuilder
    {
        private readonly IScheduleRequestService _scheduleRequestService;
        private Schedule_Template _template;
        public ScheduleBuilder(IScheduleRequestService scheduleRequestService, int creatorId)
        {
            _scheduleRequestService = scheduleRequestService;
            _template = new Schedule_Template();
            _template.CreatorId = creatorId;
        }
        public void AddName(string name)
        {
            _template.Name = name;
        }
        public void AddStartTime(TimeSpan timeSpan)
        {
            _template.StartTime = timeSpan;
        }
        public void AddEndTime(TimeSpan timeSpan)
        {
            _template.EndTime = timeSpan;
        }
        public void AddDaysOfWeek(string bitWeek)
        {
            _template.DaysOfWeekBits = bitWeek;
        }
        public async Task AddToDataBase(CancellationToken ct)
        {
            await _scheduleRequestService.CreateScheduleTemplateAsync(_template, ct);
        }
    }
}