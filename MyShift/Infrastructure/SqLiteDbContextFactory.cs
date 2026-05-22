using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyShift.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Infrastructure
{
    internal class SqLiteDbContextFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var projectPath = Directory.GetCurrentDirectory();
            // Если мы в bin/Debug, поднимаемся наверх
            if (projectPath.Contains("bin\\Debug") || projectPath.Contains("bin/Release"))
            {
                projectPath = Path.GetFullPath(Path.Combine(projectPath, "..", "..", ".."));
            }
            var dbPath = Path.Combine(projectPath, "mydb.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
