using Microsoft.EntityFrameworkCore;
using Thryft.Data;

namespace ThryftTests.Data;

public class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:") // Use SQLite in-memory instead
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection(); // Required for SQLite in-memory
        context.Database.EnsureCreated();
        return context;
    }
}