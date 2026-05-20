using MyShift.Core.Entities;
using MyShift.Core.Enums;
using MyShift.Core.Extensions;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System.Text.Json;

namespace MyShift.Core.Services
{
    internal class ScheduleRequestService : IScheduleRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IScheduleRepository _scheduleRepository;
        public ScheduleRequestService(IRequestRepository requestRepository, IScheduleRepository scheduleRepository)
        {
            _requestRepository = requestRepository;
            _scheduleRepository = scheduleRepository;
        }
        /// <summary>
        /// Создать заявку
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task InsertRequestAsync(int userId, string message, CancellationToken ct)
        {
            ValidateString(message);
            Request request = new Request(userId, message);
            await _requestRepository.InsertRequestAsync(request,ct);
        }
        /// <summary>
        /// Создать график
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="template"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UserSchedule> InsertScheduleAsync(UserSchedule schedule, ScheduleTemplate template, CancellationToken ct)
        {
            if (template.Type == 0)
            {
                DayTemplate dayTemplate = JsonSerializer.Deserialize<DayTemplate>(template.RulesJson);
                string firstDay = ((Weekday)Int32.Parse(dayTemplate.Days.Split(',')[0])).GetDisplayShortName().ToLower();
                DateTime firstWorkDay = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(1);
                while (firstWorkDay.ToString("ddd") != firstDay)
                {//делаем расчёт даты начала действия графика
                    firstWorkDay = firstWorkDay.AddDays(1);
                }
                schedule.StartDate = firstWorkDay;
                schedule.EndDate = firstWorkDay.AddMonths(template.SchedulePeriod);
                await _scheduleRepository.InsertScheduleAsync(schedule, ct);
                await GenerationDayShiftsAsync(schedule, template.RulesJson, firstWorkDay, schedule.EndDate, ct);
            }
            else if (template.Type == 1)
            {
                schedule.StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day).AddDays(1);
                schedule.EndDate = schedule.StartDate.AddMonths(template.SchedulePeriod);
                await _scheduleRepository.InsertScheduleAsync(schedule, ct);
                await GenerationCycleShiftsAsync(schedule, template.RulesJson, schedule.StartDate, schedule.EndDate, ct);
            }
            return await _scheduleRepository.GetActiveScheduleByUserAsync(schedule.UserId, ct);
        }
        /// <summary>
        /// Получить все шаблоны графиков
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct)
        {
            return await _scheduleRepository.GetAllTemplatesAsync(ct);
        }
        /// <summary>
        /// Создать шаблон графика
        /// </summary>
        /// <param name="schTemplate"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task InsertScheduleTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct)
        {
            await _scheduleRepository.InsertTemplateAsync(schTemplate, ct);
        }
        /// <summary>
        /// Удалить заявку
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task DeleteRequestAsync(int requestId, CancellationToken ct)
        {
            await _requestRepository.DeleteRequestAsync(requestId,ct);
        }
        /// <summary>
        /// Получить заявку
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Request?> GetRequestAsync(int requestId, CancellationToken ct)
        {
            return await _requestRepository.GetRequestAsync(requestId, ct);
        }
        /// <summary>
        /// Получить все заявки пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct)
        {
            return await _requestRepository.GetRequestsByUserIdAsync(userId,ct);
        }
        /// <summary>
        /// Проверка строки на пустоту
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не должна быть пустой");
        }
        /// <summary>
        /// Получить шаблон
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ScheduleTemplate?> GetTemplateAsync(int templateId, CancellationToken ct)
        {
            return await _scheduleRepository.GetScheduleTemplateAsync(templateId, ct);
        }
        /// <summary>
        /// Получить график
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UserSchedule?> GetScheduleAsync(int scheduleId, CancellationToken ct)
        {
            return await _scheduleRepository.GetScheduleAsync(scheduleId, ct);
        }
        /// <summary>
        /// Получить текущий график пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct)
        {
            return await _scheduleRepository.GetActiveScheduleByUserAsync(userId, ct);
        }
        /// <summary>
        /// Получить смену
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken ct)
        {
            return await _scheduleRepository.GetShiftByIdAsync(shiftId, ct);
        }
        /// <summary>
        /// Отредактировать время смены
        /// </summary>
        /// <param name="shiftId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Shift?> EditShiftScheduleAsync(int shiftId, TimeSpan startTime, TimeSpan endTime, CancellationToken ct)
        {
            return await _scheduleRepository.EditShiftScheduleAsync(shiftId, startTime, endTime, ct);
        }
        /// <summary>
        /// Получить список активных графиков
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct)
        {
            return await _scheduleRepository.GetActiveSchedulesAsync(ct);
        }
        /// <summary>
        /// Удалить график
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<UserSchedule?> DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct)
        {
            await _scheduleRepository.DeleteScheduleByScheduleIdAsync(scheduleId, ct);
            return await _scheduleRepository.GetScheduleAsync(scheduleId, ct);
        }
        /// <summary>
        /// Удалить смену
        /// </summary>
        /// <param name="shiftId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct)
        {
            await _scheduleRepository.DeleteShiftByShiftIdAsync(shiftId, ct);
        }
        /// <summary>
        /// Получить активные заявки
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct)
        {
            return await _requestRepository.GetActiveRequestsAsync(ct);
        }
        /// <summary>
        /// Сгенерировать смены по линейному графику
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="rulesJson"></param>
        /// <param name="firstWorkDay"></param>
        /// <param name="lastWorkDay"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<DateTime> GenerationDayShiftsAsync(UserSchedule schedule, string rulesJson, DateTime firstWorkDay, DateTime lastWorkDay, CancellationToken ct)
        {
            DayTemplate dayTemplate = JsonSerializer.Deserialize<DayTemplate>(rulesJson);
            int[] shifts = dayTemplate.Days.Split(',').Select(d => Int32.Parse(d)).ToArray();
            var shiftList = new List<Shift>();
            DateTime workDay = new DateTime(firstWorkDay.Year, firstWorkDay.Month, firstWorkDay.Day);
            int i = Array.FindIndex(shifts, s => s == (int)Enum.GetValues<Weekday>().FirstOrDefault(w => w.GetDisplayShortName().ToLower() == workDay.ToString("ddd").ToLower()));
            while (workDay <= lastWorkDay)
            {//расчитываем смены
                string dayWeekStr = workDay.ToString("ddd").ToLower();
                Weekday weekDay = Enum.GetValues<Weekday>().Cast<Weekday>().ToArray().FirstOrDefault(wd => wd.GetDisplayShortName().ToLower() == dayWeekStr);
                if (shifts[i] == (int)weekDay)
                {
                    shiftList.Add(new Shift(schedule.Id, workDay, dayTemplate.Start, dayTemplate.End, dayTemplate.Type, true));
                    if (i == shifts.Length - 1)
                        i = 0;
                    else
                        i++;
                }
                workDay = workDay.AddDays(1);
            }
            await _scheduleRepository.InstertShiftsAsync(shiftList, ct);
            return firstWorkDay;
        }
        /// <summary>
        /// Сгенерировать смены по цикличному графику
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="rulesJson"></param>
        /// <param name="firstWorkDay"></param>
        /// <param name="lastWorkDay"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<DateTime> GenerationCycleShiftsAsync(UserSchedule schedule, string rulesJson, DateTime firstWorkDay, DateTime lastWorkDay, CancellationToken ct)
        {
            CycleTemplate cycleTemplate = JsonSerializer.Deserialize<CycleTemplate>(rulesJson);
            int i = 0;
            var shiftList = new List<Shift>();
            while (firstWorkDay <= lastWorkDay)
            {
                shiftList.Add(new Shift(schedule.Id, firstWorkDay, cycleTemplate[i].Start, cycleTemplate[i].End, cycleTemplate[i].TypeShift, true));
                if (i == cycleTemplate.Count - 1)
                {
                    i = 0;
                }
                else
                    i++;
                firstWorkDay = firstWorkDay.AddDays(1);
            }
            await _scheduleRepository.InstertShiftsAsync(shiftList, ct);
            return firstWorkDay;
        }
        /// <summary>
        /// Отказать в заявке
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="processorId"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Request> RejectRequestAsync(int requestId, int processorId, string message, CancellationToken ct)
        {
            return await _requestRepository.RejectRequestAsync(requestId, processorId, message, ct);
        }
        /// <summary>
        /// Принять заявку
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="processorId"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Request> ApproveRequestAsync(int requestId, int processorId, string message, CancellationToken ct)
        {
            return await _requestRepository.ApproveRequestAsync(requestId, processorId, message, ct);
        }
        /// <summary>
        /// Установить пользователя, который будет отвечать на текущую заявку
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="processorId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task SetProcessorAsync(int requestId, int processorId, CancellationToken ct)
        {
            await _requestRepository.SetProcessorAsync(requestId, processorId, ct);
        }
    }
}