

using Microsoft.EntityFrameworkCore;
using MyShift.Core.Entities;
using MyShift.Core.Models;

namespace MyShift.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ToDoUser> Users { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<UserSchedule> UserSchedules { get; set; }
        public DbSet<ScheduleTemplate> Schedule_Templates { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var projectPath = Directory.GetCurrentDirectory();
                if (projectPath.Contains("bin\\Debug") || projectPath.Contains("bin/Release"))
                    projectPath = Path.GetFullPath(Path.Combine(projectPath, "..", "..", ".."));

                var dbPath = Path.Combine(projectPath, "mydb.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }
    }
}