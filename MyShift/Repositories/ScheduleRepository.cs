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
    internal class ScheduleRepository : IScheduleRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public ScheduleRepository(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<UserSchedule?> InsertScheduleAsync(UserSchedule schedule, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.UserSchedules.AddAsync(schedule, ct);
            await context.SaveChangesAsync(ct);
            return schedule;
        }
        public async Task InsertTemplateAsync(ScheduleTemplate schTemplate, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.AddAsync(schTemplate,ct);
            await context.SaveChangesAsync(ct);
        }

        public async Task InsertSchedule_TemplateAsync(ScheduleTemplate_Schedule templ_schedule, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.AddAsync(templ_schedule, ct);
            await context.SaveChangesAsync(ct);
        }

        public async Task InstertShiftsAsync(List<Shift> shifts, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.AddRangeAsync(shifts, ct);
            await context.SaveChangesAsync(ct);
        }

        public async Task<Shift?> EditShiftScheduleAsync(int shiftId, TimeSpan startTime, TimeSpan endTime, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Shift? shift = await context.Shifts
                .Include(s => s.UserSchedule)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == shiftId, ct);
            if (shift == null)
                return null;
            shift.StartTime = startTime;
            shift.EndTime = endTime;
            await context.SaveChangesAsync(ct);
            return shift;
        }

        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Schedule_Templates.ToListAsync(ct);
        }

        public async Task<UserSchedule?> GetScheduleAsync(int id, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.UserSchedules
                .Include(sch => sch.User)
                .Include(sch => sch.Template)
                .Include(sch => sch.Shifts)
                .FirstOrDefaultAsync(sch => sch.Id == id, ct);
        }

        public async Task<ScheduleTemplate?> GetScheduleTemplateAsync(int id, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Schedule_Templates.FirstOrDefaultAsync(tmp => tmp.Id == id, ct);
        }

        public async Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.UserSchedules
                .AsNoTracking()
                .Include(us => us.User)
                .Include(us => us.AssignedBy)
                .Include(us => us.Template)
                .Include(us => us.Shifts.Where(s => s.Status == true))
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive == true, ct);
        }

        public async Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Shifts
                .Include(sch => sch.UserSchedule)
                .ThenInclude(sch => sch.User)
                .FirstOrDefaultAsync(sch => sch.Id == shiftId, ct);
        }

        public async Task<IReadOnlyList<Shift>> GetAllShiftsForTomorrowAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Shifts
                .Include(s => s.UserSchedule)
                    .ThenInclude(s => s.User)
                .Where(s => s.Status == true && s.ShiftDate.Date == DateTime.UtcNow.Date.AddDays(1) && s.ShiftType != ShiftType.off).ToListAsync(ct);
        }
        public async Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.UserSchedules
                .AsNoTracking()
                .Include(us => us.User)
                .Include(us => us.AssignedBy)
                .Include(us => us.Template)
                .Include(us => us.Shifts.Where(s => s.Status == true))
                .Where(us => us.IsActive == true).ToListAsync(ct);
        }

        public async Task DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            UserSchedule schedule = await context.UserSchedules.FirstOrDefaultAsync(sch => sch.Id == scheduleId, ct);
            schedule.IsActive = false;
            await context.SaveChangesAsync(ct);
        }

        public async Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            Shift shift = await context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId, ct);
            shift.Status = false;
            await context.SaveChangesAsync(ct);
        }

        public async Task<Shift?> GetLastShiftsByScheduleIdAsync(int scheduleId, CancellationToken ct)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Shifts.Where(s => s.UserScheduleId == scheduleId)
                .Include(s => s.UserSchedule)
                .OrderByDescending(s => s.ShiftDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}