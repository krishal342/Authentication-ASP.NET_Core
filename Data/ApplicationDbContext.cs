using Authentication.Models;
using Microsoft.EntityFrameworkCore;


namespace Authentication.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<UnverifiedUser> UnverifiedUsers { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Otp> Otps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

    }

}
