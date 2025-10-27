using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend_EAD.Models;


namespace AutoServiceBackend.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        // Add your tables here
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }

}
