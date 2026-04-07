using MyShift.Core.Helpers;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
            
        }
        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplates()
        {
            return await _scheduleRepository.GetAllTemplates();
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
    }
}