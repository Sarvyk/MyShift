using MyShift.Core.Entities;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MyShift.BackgroundTasks
{
    internal class AdditionalGenerationSchedule : BackgroundTask
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly INotificationService _notificationService;

        public AdditionalGenerationSchedule(TimeSpan delay, ITelegramBotClient botClient, IScheduleRequestService scheduleRequestService, INotificationService notificationService) : base(delay, nameof(AdditionalGenerationSchedule))
        {
            _botClient = botClient;
            _scheduleRequestService = scheduleRequestService;
            _notificationService = notificationService;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = (await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct)).Where(n => n.Type.StartsWith("ScheduleExtension_"));
            foreach (Notification notification in notifications)
            {
                UserSchedule userSchedule = await _scheduleRequestService.GetScheduleAsync(Int32.Parse(notification.Type.Split("_")[1].ToString()), ct);
                if (userSchedule.IsActive)
                {
                    Shift lastShift = userSchedule.Shifts.OrderByDescending(s => s.ShiftDate).FirstOrDefault();
                    if (userSchedule.Template.Type == 0)
                    {
                        DayTemplate template = JsonSerializer.Deserialize<DayTemplate>(userSchedule.Template.RulesJson);
                        Weekday[] sequenceWeekdays = template.Days.Split(",").Select(d => (Weekday)Enum.Parse(typeof(Weekday), d)).ToArray();
                        Weekday lastWeekdayWork = sequenceWeekdays.FirstOrDefault(w => w.GetDisplayShortName().ToLower() == lastShift.ShiftDate.ToString("ddd").ToLower());
                        Weekday newFirstWorkWeekDay = Weekday.none;
                        for (int i = 0; i < sequenceWeekdays.Length; i++)
                        {
                            // находим следующий день недели
                            if (sequenceWeekdays[i] == lastWeekdayWork && i == sequenceWeekdays.Length - 1)
                            {
                                newFirstWorkWeekDay = sequenceWeekdays[0];
                                break;
                            }
                            else if (sequenceWeekdays[i] == lastWeekdayWork && i != sequenceWeekdays.Length - 1)
                            {
                                newFirstWorkWeekDay = sequenceWeekdays[i + 1];
                                break;
                            }
                        }
                        DateTime newScheduleFirstDay = lastShift.ShiftDate.AddDays(1);
                        while (newScheduleFirstDay.ToString("ddd").ToLower() != newFirstWorkWeekDay.GetDisplayShortName().ToLower())
                        {
                            // находим по дню недели следующий день смены
                            newScheduleFirstDay = newScheduleFirstDay.AddDays(1);
                        }
                        DateTime newLastDate = await _scheduleRequestService.GenerationDayShifts(userSchedule, userSchedule.Template.RulesJson, newScheduleFirstDay, newScheduleFirstDay.AddMonths(userSchedule.Template.SchedulePeriod), ct);
                        await _botClient.SendMessage(userSchedule.User.TelegramId, notification.Text + newLastDate.Date.ToShortDateString(), cancellationToken: ct);
                        await _notificationService.MarkNotified(notification.id, ct);
                    }
                    else if (userSchedule.Template.Type == 1)
                    {
                        // Тут просто сделаем первый день выходной, а со следующего последовательность начнётся сначала.
                        DateTime firstWorkDay = lastShift.ShiftDate.AddDays(2);
                        DateTime lastWorkDay = firstWorkDay.AddMonths(userSchedule.Template.SchedulePeriod);
                        DateTime newLastDate = await _scheduleRequestService.GenerationCycleShifts(userSchedule, userSchedule.Template.RulesJson, firstWorkDay, lastWorkDay, ct);
                        await _botClient.SendMessage(userSchedule.User.TelegramId, notification.Text + newLastDate.Date.ToShortDateString(), cancellationToken: ct);
                        await _notificationService.MarkNotified(notification.id, ct);
                    }
                }
                else
                    await _notificationService.MarkNotified(notification.id, ct);
            }
        }
    }
}