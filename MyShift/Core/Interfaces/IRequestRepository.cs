using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Interfaces
{
    public interface IRequestRepository
    {
        Task<Request> InsertRequestAsync(Request request, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetRequestsByUserIdAsync(int userId, CancellationToken ct);
        Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct);
        Task<Request?> GetRequestAsync(int requestId, CancellationToken ct);
        Task SetProcessorAsync(int requestId, int processorId, CancellationToken ct);
        Task<Request> ApproveRequestAsync(int requestId, int processorId, string message, CancellationToken ct);
        Task<Request> RejectRequestAsync(int requestId, int processorId, string message, CancellationToken ct);
        Task DeleteRequestAsync(int requestId, CancellationToken ct);
    }
}
