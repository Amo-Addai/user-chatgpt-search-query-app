using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using ChatGPTBackend.Models;

namespace ChatGPTBackend.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User>? Users { get; set; }
        public DbSet<Query>? Queries { get; set; }
    }

    public class AppIdDbContext : IdentityDbContext // todo: <User> | User : TUser first
    {
        public AppIdDbContext(DbContextOptions<AppIdDbContext> options)
            : base(options)
        {}

        public DbSet<User>? IdUsers { get; set; }

        // Override OnModelCreating if needed
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customizations for entity configurations
        }
    }

}
