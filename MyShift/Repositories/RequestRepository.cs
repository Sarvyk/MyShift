using MyShift.Data;
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
        public void ApproveRequest()
        {
            throw new NotImplementedException();
        }

        public void GetRequest()
        {
            throw new NotImplementedException();
        }

        public void RejectRequest()
        {
            throw new NotImplementedException();
        }
    }
}
