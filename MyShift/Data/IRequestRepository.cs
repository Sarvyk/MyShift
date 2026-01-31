using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Data
{
    public interface IRequestRepository
    {
        Task CreateRequestAsync(Request request);
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId);
        Task<Request?> GetRequestAsync(int userId, int requestId);
        Task ApproveRequestAsync();
        Task RejectRequestAsync();
        Task DeleteRequestAsync(int requestId);
    }
}
