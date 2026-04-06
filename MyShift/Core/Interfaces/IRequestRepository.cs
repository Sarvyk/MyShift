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
        Task<IReadOnlyList<Request>> GetRequestsAsync(int userId, CancellationToken ct);
        Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct);
        Task ApproveRequestAsync( CancellationToken ct);
        Task RejectRequestAsync( CancellationToken ct);
        Task DeleteRequestAsync(int requestId, CancellationToken ct);
    }
}
