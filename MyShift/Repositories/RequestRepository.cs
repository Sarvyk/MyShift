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
        public async Task<Request> ApproveRequestAsync(int requestId, int processorId, string message, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Request request = await context.Requests
                .Include(req => req.Creator)
                .FirstOrDefaultAsync(req => req.Id == requestId, ct);
            request.ProcessorId = processorId;
            request.ResolutionComment = message;
            request.Status = RequestStatus.Pending;
            request.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
            return request;
        }

        public async Task<Request> RejectRequestAsync(int requestId, int processorId, string message, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Request request = await context.Requests
                .Include(req => req.Creator)
                .FirstOrDefaultAsync(req => req.Id == requestId, ct);
            request.ProcessorId = processorId;
            request.ResolutionComment = message;
            request.Status = RequestStatus.Rejected;
            request.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
            return request;
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
            Request? request = await context.Requests.FindAsync(requestId,ct);
            request.Status = RequestStatus.Removed;
            await context.SaveChangesAsync(ct);
        }

        public async Task<Request?> GetRequestAsync(int requestId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests
                .Include(req => req.Creator)
                .Include(req => req.Processor)
                .FirstOrDefaultAsync(req => req.Id == requestId,ct);
        }

        public async Task<IReadOnlyList<Request>> GetRequestsByUserIdAsync(int userId , CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests
                .Where(req => req.CreatorId == userId && req.Status != RequestStatus.Removed && req.ProcessorId == null)
                .OrderByDescending(req => req.CreatedAt).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Request>> GetActiveRequestsAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Requests.Where(req => req.Status == RequestStatus.Pending).ToListAsync(ct);
        }

        public async Task SetProcessorAsync(int requestId, int processorId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Request request = await context.Requests.FirstOrDefaultAsync(req => req.Id == requestId, ct);
            request.ProcessorId = processorId;
            await context.SaveChangesAsync(ct);
        }
    }
}