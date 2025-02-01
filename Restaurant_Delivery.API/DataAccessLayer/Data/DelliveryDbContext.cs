using Microsoft.EntityFrameworkCore;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.DataAccessLayer.Data
{
    public class DeliveryDbContext : DbContext
    {
        // DbSet properties for entities
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DishBasket> DishBaskets { get; set; }

        // Constructor for DbContext configuration
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
            : base(options)
        {
        }

        // Configure entity relationships and properties
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dish entity configuration
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(255);
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.Price).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(d => d.Rating).HasColumnType("decimal(18, 2)");
                entity.Property(d => d.Image).IsRequired();
                entity.Property(d => d.Category).HasMaxLength(100);
            });

            // Order entity configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Price).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(o => o.Status).HasMaxLength(50);
                entity.HasMany(o => o.Dishes)
                    .WithOne(d => d.Order)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DishBasket entity configuration
            modelBuilder.Entity<DishBasket>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(255);
                entity.Property(d => d.Price).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(d => d.TotalPrice).HasColumnType("decimal(18, 2)").IsRequired();
                entity.Property(d => d.Amount).IsRequired();
                entity.Property(d => d.Image).IsRequired();
                entity.Property(d => d.UserId).IsRequired();
                entity.HasIndex(d => d.OrderId);
            });
        }
    }
}