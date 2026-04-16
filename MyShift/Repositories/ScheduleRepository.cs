using Microsoft.EntityFrameworkCore;
using MyShift.Core.Data;
using MyShift.Core.Interfaces;
using MyShift.Core.Models;
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
        public async Task<UserSchedule?> InsertScheduleAsync(UserSchedule schedule, CancellationToken ct)
        {
            await _context.Schedules.AddAsync(schedule, ct);
            await _context.SaveChangesAsync(ct);
            return schedule;
        }
        public async Task InsertTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct)
        {
            await _context.AddAsync(schTemplate,ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InsertSchedule_TemplateAsync(ScheduleTemplate_Schedule templ_schedule, CancellationToken ct)
        {
            await _context.AddAsync(templ_schedule);
            await _context.SaveChangesAsync(ct);
        }

        public async Task InstertShiftsAsync(List<Shift> shifts, CancellationToken ct)
        {
            await _context.AddRangeAsync(shifts);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteScheduleAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task EditShiftScheduleAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct)
        {
            return await _context.Schedule_Templates.ToListAsync();
        }

        public async Task<UserSchedule?> GetScheduleAsync(int id, CancellationToken ct)
        {
            return await _context.Schedules.FirstOrDefaultAsync(schId => schId.Id == id, cancellationToken: ct);
        }

        public async Task<ScheduleTemplate?> GetScheduleTemplateAsync(int id, CancellationToken ct)
        {
            return await _context.Schedule_Templates.FirstOrDefaultAsync(tmp => tmp.Id == id, cancellationToken: ct);
        }

        public async Task<UserSchedule?> GetActiveScheduleByUser(int userId, CancellationToken ct)
        {
            return await _context.Schedules.FirstOrDefaultAsync(sch => sch.UserId == userId && sch.IsActive == true);
        }
    }
}