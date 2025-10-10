using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Thryft.Models;

namespace Thryft.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        :base(options)
    {

    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=app.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Created).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Status).HasMaxLength(50);

            // Relationship with User
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure OrderItem entity (composite key)
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId });

            // Relationships
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.OrderId);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            // Configure Colours array - store as comma-separated string
            entity.Property(p => p.Colours)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(c => Enum.Parse<Colour>(c.Trim()))
                          .ToArray()
                )
                .HasColumnType("nvarchar(255)");

            // Configure Sizes array - store as comma-separated string
            entity.Property(p => p.Sizes)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => Enum.Parse<Size>(s.Trim()))
                          .ToArray()
                )
                .HasColumnType("nvarchar(255)");

            // Configure Price
            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        });
    }
}
