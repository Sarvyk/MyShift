using MyShift.Data;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task CreateRequestAsync(int userId, string message, CancellationToken ct)
        {
            ValidateString(message);
            Request request = new Request(userId, message);
            await _requestRepository.CreateRequestAsync(request,ct);
        }

        public async Task CreateScheduleTemplateAsync(Schedule_Template schTemplate, CancellationToken ct)
        {
            await _scheduleRepository.CreateScheduleTemplateAsync(schTemplate, ct);
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

    }
}