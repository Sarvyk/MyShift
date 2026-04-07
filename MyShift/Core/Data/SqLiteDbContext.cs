using Microsoft.EntityFrameworkCore;
using MyShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Data
{
    public class SqLiteDbContext : DbContext
    {
        public SqLiteDbContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public DbSet<ToDoUser> Users { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<UserSchedule> Schedules { get; set; }
        public DbSet<ScheduleTemplate> Schedule_Templates { get; set; }
        public DbSet<ScheduleTemplate_Schedule> ToDoUser_ScheduleTemplates { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source=./mydb.db");
    }
}