using MyShift.Data;
using MyShift.Helpers;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyShift.Services
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
        public async Task InsertScheduleAsync(Schedule schedule, ScheduleTemplate template, CancellationToken ct)
        {//добавляем сразу массивом, чтобы сделать сразу график на некоторый срок.
            await _scheduleRepository.InsertScheduleAsync(schedule, ct);
            await _scheduleRepository.InsertSchedule_Template(new ScheduleTemplate_Schedule(template, schedule), ct);
            List<Schedule> schedules = new List<Schedule>();
            HashSet<string> Weeks = EnumBitConverter.GetEnumFromBitToMass(template.DaysOfWeekBits);
            DateTime date = DateTime.Now;
            int month = 6;
            for (int i = 0; i < month; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    string daywek = date.ToString("ddd", new CultureInfo("ru-RU"));
                    if (Weeks.Contains(daywek))
                    {
                        schedules.Add(schedule.Clone(date));
                    }
                    date = date.AddDays(1);
                }
            }
            await _scheduleRepository.InsertScheduleRangeAsync(schedules, ct);
        }
        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplates()
        {
            return await _scheduleRepository.GetAllTemplates();
        }
        public async Task InsertScheduleTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct)
        {
            await _scheduleRepository.InsertTemplateAsync(schTemplate, ct);
        }

        public async Task DeleteRequestAsync(int userId, string numberRequest, CancellationToken ct)
        {
            if(await _requestRepository.GetRequestAsync(userId, ValidateInt(numberRequest),ct) != null)
            {
                await _requestRepository.DeleteRequestAsync(ValidateInt(numberRequest),ct);
            }
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
    }
}