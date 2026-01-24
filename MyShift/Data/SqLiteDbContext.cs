using Microsoft.EntityFrameworkCore;
using MyShift.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Data
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
        public DbSet<Schedule> Schedules { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source=./mydb.db");
    }
}