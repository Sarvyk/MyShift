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
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public RequestRepository(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public Task ApproveRequestAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<Request> InsertRequestAsync(Request request, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.Requests.AddAsync(request, ct);
            await context.SaveChangesAsync(ct);
            return request;
        }

        public async Task DeleteRequestAsync(int requestId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Request? request = await context.Requests.FindAsync(requestId);
            request.Status = RequestStatus.Removed;
            await context.SaveChangesAsync(ct);
        }

        public async Task<Request?> GetRequestAsync(int userId, int requestId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests.FirstOrDefaultAsync(req => req.CreatorId == userId && req.Id == requestId);
        }

        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId , CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests.Where(req => req.CreatorId == userId && req.Status != RequestStatus.Removed).OrderByDescending(req => req.CreatedAt) .ToListAsync();
        }

        public Task RejectRequestAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests.Where(req => req.Status == RequestStatus.Pending).ToListAsync();
        }
    }
}