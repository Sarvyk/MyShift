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
        public void CreateSchedule()
        {
            throw new NotImplementedException();
        }

        public void DeleteSchedule()
        {
            throw new NotImplementedException();
        }

        public void EditShiftSchedule()
        {
            throw new NotImplementedException();
        }
    }
}
