using JWT.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JWT.API.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts)
        {

        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                username = "user1",
                password = "pass1"
            }
           );
        }
    }
}
