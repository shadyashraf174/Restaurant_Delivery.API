using Microsoft.EntityFrameworkCore;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.DataAccessLayer.Data
{
    public class DeliveryAuthDBContext : DbContext
    {
        // DbSet for the User entity
        public DbSet<User> Users { get; set; }

        // Constructor to configure the DbContext
        public DeliveryAuthDBContext(DbContextOptions<DeliveryAuthDBContext> options) : base(options)
        {
        }

        // Override OnModelCreating to configure the model (if needed)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Set primary key
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(100); // Store hashed passwords
                entity.Property(u => u.Address).HasMaxLength(200);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.Gender).HasConversion<string>(); // Store Gender enum as string
            });
        }
    }
}
