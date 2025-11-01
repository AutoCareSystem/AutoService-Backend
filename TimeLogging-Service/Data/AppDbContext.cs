using System.Collections.Generic;
using TimeLogging_Service.Models;
using Microsoft.EntityFrameworkCore;


namespace TimeLogging_Service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    }
}
