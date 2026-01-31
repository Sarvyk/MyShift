using MyShift.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Repositories
{
    internal class ScheduleRepository : IScheduleRepository
    {
        private readonly SqLiteDbContext _context;
        public ScheduleRepository(SqLiteDbContext context)
        {
            _context = context;
        }
        public async Task CreateScheduleAsync()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteScheduleAsync()
        {
            throw new NotImplementedException();
        }

        public async Task EditShiftScheduleAsync()
        {
            throw new NotImplementedException();
        }
    }
}
