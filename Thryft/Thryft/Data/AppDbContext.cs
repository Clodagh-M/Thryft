using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Thryft.Models;

namespace Thryft.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Address> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Your existing model configuration remains the same
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')")
                .ValueGeneratedOnAddOrUpdate();
        });

        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.ProductId });

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Colours)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(c => Enum.Parse<Colour>(c.Trim()))
                          .ToArray()
                )
                .HasColumnType("nvarchar(255)");

            entity.Property(p => p.Sizes)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => Enum.Parse<Size>(s.Trim()))
                          .ToArray()
                )
                .HasColumnType("nvarchar(255)");

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        });
    }
}