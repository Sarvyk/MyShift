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
        public async Task InsertRequestAsync(int userId, string message, CancellationToken ct)
        {
            ValidateString(message);
            Request request = new Request(userId, message);
            await _requestRepository.InsertRequestAsync(request,ct);
        }
        public async Task InsertScheduleAsync(UserSchedule schedule, ScheduleTemplate template, CancellationToken ct)
        {
            if (template.Type == 0)
            {
                DayTemplate dayTemplate = JsonSerializer.Deserialize<DayTemplate>(template.RulesJson);
                string firstDay = ((Weekday)Int32.Parse(dayTemplate.Days.Split(',')[0])).GetDisplayShortName().ToLower();
                DateTime firstWorkDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                while (firstWorkDay.ToString("ddd") != firstDay)
                {//делаем расчёт даты начала действия графика
                    firstWorkDay = firstWorkDay.AddDays(1);
                }
                schedule.StartDate = firstWorkDay;
                schedule.EndDate = firstWorkDay.AddMonths(6);

                int[] shifts = dayTemplate.Days.Split(',').Select(d => Int32.Parse(d)).ToArray();
                var shiftList = new List<Shift>();
                DateTime workDay = new DateTime(firstWorkDay.Year, firstWorkDay.Month, firstWorkDay.Day);
                int i = 0;
                await _scheduleRepository.InsertScheduleAsync(schedule, ct);
                while (workDay <= schedule.EndDate)
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
            }
            else if (template.Type == 1)
            {
                CycleTemplate cycleTemplate = JsonSerializer.Deserialize<CycleTemplate>(template.RulesJson);
                schedule.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                schedule.EndDate = schedule.StartDate.AddMonths(6);
                await _scheduleRepository.InsertScheduleAsync(schedule, ct);
                DateTime workDay = schedule.StartDate;
                int i = 0;
                var shiftList = new List<Shift>();
                while (workDay <= schedule.EndDate)
                {
                    shiftList.Add(new Shift(schedule.Id, workDay, cycleTemplate[i].Start, cycleTemplate[i].End, cycleTemplate[i].TypeShift, true));
                    if (i == cycleTemplate.Count-1)
                    {
                        i = 0;
                    }
                    else
                        i++;
                    workDay = workDay.AddDays(1);
                }
                await _scheduleRepository.InstertShiftsAsync(shiftList, ct);
            }
        }
        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct)
        {
            return await _scheduleRepository.GetAllTemplatesAsync(ct);
        }
        public async Task InsertScheduleTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct)
        {
            await _scheduleRepository.InsertTemplateAsync(schTemplate, ct);
        }

        public async Task DeleteRequestAsync(int requestId, CancellationToken ct)
        {
            await _requestRepository.DeleteRequestAsync(requestId,ct);
        }

        public async Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct)
        {
            return await _requestRepository.GetRequestAsync(userId, requestId, ct);
        }

        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct)
        {
            return await _requestRepository.GetRequestsAsync(userId,ct);
        }

        public async Task GetScheduleAsync(ToDoUser toDoUser, CancellationToken ct)
        {//Тут нужно подумать. Пока что не очень понимаю как сделать лучше.
            throw new NotImplementedException();
        }
        private int ValidateInt(string? str)
        {
            if (int.TryParse(str, out int result))
            {
                return result;
            }
            else
                throw new ArgumentException("Строка не должна быть пустой и должна состоять из цифр");
        }
        public void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не должна быть пустой");
        }

        public async Task<ScheduleTemplate?> GetTemplateAsync(int templateId, CancellationToken ct)
        {
            return await _scheduleRepository.GetScheduleTemplateAsync(templateId, ct);
        }

        public async Task<UserSchedule?> GetScheduleAsync(int scheduleId, CancellationToken ct)
        {
            return await _scheduleRepository.GetScheduleAsync(scheduleId, ct);
        }

        public async Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct)
        {
            return await _scheduleRepository.GetActiveScheduleByUserAsync(userId, ct);
        }

        public async Task<Shift?> GetShiftByIdAsync(int scheduleId, CancellationToken ct)
        {
            return await _scheduleRepository.GetShiftByIdAsync(scheduleId, ct);
        }

        public async Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct)
        {
            return await _scheduleRepository.GetActiveSchedulesAsync(ct);
        }

        public async Task DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct)
        {
            await _scheduleRepository.DeleteScheduleByScheduleIdAsync(scheduleId, ct);
        }

        public async Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct)
        {
            await _scheduleRepository.DeleteShiftByShiftIdAsync(shiftId, ct);
        }
    }
}