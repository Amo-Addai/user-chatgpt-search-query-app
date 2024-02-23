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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configure database connection
            optionsBuilder.UseSqlite("Data Source=Data/sqlite.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* // todo: Test
            // Set Id as Primary Key
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<Query>().HasKey(q => q.Id);

            // Set Id Primary Key to Auto-increment
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd(); 
            modelBuilder.Entity<Query>().Property(q => q.Id).ValueGeneratedOnAdd();

            // Configure entity relationships
            modelBuilder.Entity<User>().HasMany(u => u.Queries).WithOne(q => q.UserId);
            
            // Seed data (optional)
            modelBuilder.Entity<User>().HasData(
                new User { Username = "user1", Password = "password1" },
                new User { Username = "user2", Password = "password2" }
            );
            modelBuilder.Entity<Query>().HasData(
                new Query { QueryText = "query1", ResponseText = "response1" },
                new Query { QueryText = "query2", ResponseText = "response2" }
            );
            */
        }
    }

    /* // TODO: Try both DbContexts
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
    */

}
