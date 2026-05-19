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
        private readonly SqLiteDbContext _context;
        public ScheduleRepository(SqLiteDbContext context)
        {
            _context = context;
        }
        public async Task<UserSchedule?> InsertScheduleAsync(UserSchedule schedule, CancellationToken ct)
        {
            await _context.UserSchedules.AddAsync(schedule, ct);
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

        public async Task<Shift?> EditShiftScheduleAsync(int shiftId, TimeSpan startTime, TimeSpan endTime, CancellationToken ct)
        {
            Shift? shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
            if (shift == null)
                return null;
            shift.StartTime = startTime;
            shift.EndTime = endTime;
            await _context.SaveChangesAsync(ct);
            return shift;
        }

        public async Task<IReadOnlyList<ScheduleTemplate>> GetAllTemplatesAsync(CancellationToken ct)
        {
            return await _context.Schedule_Templates.ToListAsync();
        }

        public async Task<UserSchedule?> GetScheduleAsync(int id, CancellationToken ct)
        {
            return await _context.UserSchedules
                .Include(sch => sch.User)
                .Include(sch => sch.Template)
                .Include(sch => sch.Shifts)
                .FirstOrDefaultAsync(sch => sch.Id == id, cancellationToken: ct);
        }

        public async Task<ScheduleTemplate?> GetScheduleTemplateAsync(int id, CancellationToken ct)
        {
            return await _context.Schedule_Templates.FirstOrDefaultAsync(tmp => tmp.Id == id, cancellationToken: ct);
        }

        public async Task<UserSchedule?> GetActiveScheduleByUserAsync(int userId, CancellationToken ct)
        {
            return await _context.UserSchedules
                .AsNoTracking()
                .Include(us => us.User)
                .Include(us => us.AssignedBy)
                .Include(us => us.Template)
                .Include(us => us.Shifts.Where(s => s.Status == true))
                .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive == true);
        }

        public async Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken ct)
        {
            return await _context.Shifts
                .Include(sch => sch.UserSchedule)
                .ThenInclude(sch => sch.User)
                .FirstOrDefaultAsync(sch => sch.Id == shiftId);
        }

        public async Task<IReadOnlyList<Shift>> GetAllShiftsForTomorrow(CancellationToken ct)
        {
            return await _context.Shifts
                .Include(s => s.UserSchedule)
                    .ThenInclude(s => s.User)
                .Where(s => s.Status == true && s.ShiftDate.Date == DateTime.UtcNow.Date.AddDays(1) && s.ShiftType != ShiftType.off).ToListAsync();
        }
        public async Task<IReadOnlyList<UserSchedule>> GetActiveSchedulesAsync(CancellationToken ct)
        {
            return await _context.UserSchedules
                .AsNoTracking()
                .Include(us => us.User)
                .Include(us => us.AssignedBy)
                .Include(us => us.Template)
                .Include(us => us.Shifts.Where(s => s.Status == true))
                .Where(us => us.IsActive == true).ToListAsync(ct);
        }

        public async Task DeleteScheduleByScheduleIdAsync(int scheduleId, CancellationToken ct)
        {
            UserSchedule schedule = await _context.UserSchedules.FirstOrDefaultAsync(sch => sch.Id == scheduleId);
            schedule.IsActive = false;
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteShiftByShiftIdAsync(int shiftId, CancellationToken ct)
        {
            Shift shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
            shift.Status = false;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Shift?> GetLastShiftsByScheduleId(int scheduleId, CancellationToken ct)
        {
            return await _context.Shifts.Where(s => s.UserScheduleId == scheduleId)
                .Include(s => s.UserSchedule)
                .OrderByDescending(s => s.ShiftDate)
                .FirstOrDefaultAsync(ct);
        }
    }
}