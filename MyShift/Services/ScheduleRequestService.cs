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
        IRequestRepository _requestRepository;
        IScheduleRepository _scheduleRepository;
        public ScheduleRequestService(IRequestRepository requestRepository, IScheduleRepository scheduleRepository)
        {
            _requestRepository = requestRepository;
            _scheduleRepository = scheduleRepository;
        }
        public async Task CreateRequestAsync(int userId, string message)
        {
            ValidateString(message);
            Request request = new Request(userId, message);
            await _requestRepository.CreateRequestAsync(request);
        }

        public async Task DeleteRequestAsync(int userId, string numberRequest)
        {
            if(_requestRepository.GetRequestAsync(userId, ValidateInt(numberRequest)).Result != null)
            {
                await _requestRepository.DeleteRequestAsync(ValidateInt(numberRequest));
            }
        }

        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId)
        {
            return _requestRepository.GetRequestsAsync(userId).Result;
        }

        public async Task GetScheduleAsync(ToDoUser toDoUser)
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
        private void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не должна быть пустой");
        }
    }
}