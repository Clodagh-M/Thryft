using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Thryft.Models;

namespace Thryft.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base(options)
    {

    }

    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
}
