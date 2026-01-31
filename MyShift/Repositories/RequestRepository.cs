using Microsoft.EntityFrameworkCore;
using MyShift.Data;
using MyShift.Enums;
using MyShift.Models;
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
        public Task ApproveRequestAsync()
        {
            throw new NotImplementedException();
        }

        public async Task CreateRequestAsync(Request request)
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequestAsync(int requestId)
        {
            Request request = await _context.Requests.FindAsync(requestId);
            request.Status = RequestStatus.Removed;
            await _context.SaveChangesAsync();
        }

        public async Task<Request?> GetRequestAsync(int userId, int requestId)
        {
            return await _context.Requests.FirstOrDefaultAsync(req => req.CreatorKey == userId && req.Id == requestId);
        }

        public async Task<IReadOnlyList<Request>> GetRequestsAsync(int userId)
        {
            return await _context.Requests.Where(req => req.CreatorKey == userId).ToListAsync();
        }

        public Task RejectRequestAsync()
        {
            throw new NotImplementedException();
        }
    }
}
