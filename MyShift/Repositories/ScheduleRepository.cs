using MyShift.Data;
using MyShift.Models;
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
        public async Task CreateScheduleAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task CreateScheduleTemplateAsync(Schedule_Template schTemplate, CancellationToken ct)
        {
            await _context.AddAsync(schTemplate,ct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task EditShiftScheduleAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
