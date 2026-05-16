using Microsoft.EntityFrameworkCore;
using MyShift.Core.Data;
using MyShift.Core.Enums;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Repositories
{
    internal class RequestRepository : IRequestRepository
    {
        private readonly SqLiteDbContext _context;
        public RequestRepository(SqLiteDbContext context)
        {
            _context = context;
        }
        public Task ApproveRequestAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<Request> InsertRequestAsync(Request request, CancellationToken ct)
        {
            await _context.Requests.AddAsync(request, ct);
            await _context.SaveChangesAsync(ct);
            return request;
        }

        public async Task DeleteRequestAsync(int requestId, CancellationToken ct)
        {
            Request? request = await _context.Requests.FindAsync(requestId);
            request.Status = RequestStatus.Removed;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct)
        {
            return await _context.Requests.FirstOrDefaultAsync(req => req.CreatorId == userId && req.Id == requestId);
        }

        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId , CancellationToken ct)
        {
            return await _context.Requests.Where(req => req.CreatorId == userId && req.Status != RequestStatus.Removed).OrderByDescending(req => req.CreatedAt) .ToListAsync();
        }

        public Task RejectRequestAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct)
        {
            return await _context.Requests.Where(req => req.Status == RequestStatus.Pending).ToListAsync();
        }
    }
}