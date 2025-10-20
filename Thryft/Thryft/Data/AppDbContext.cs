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
    public DbSet<Address> Addresses { get; set; }
    //public DbSet<CartItem> CartItems { get; set; }

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
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.AddressLine1).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.County).IsRequired();
            entity.Property(e => e.Eircode).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");

            // Relationship with User
            entity.HasOne(a => a.User)
                  .WithMany(u => u.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderItem composite primary key
        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.ProductId });

        // Configure Order -> OrderItem relationship
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure OrderItem -> Product relationship
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany() // Adjust if Product has navigation property back to OrderItems
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Order -> User relationship
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany() // Adjust if User has navigation property back to Orders
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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
