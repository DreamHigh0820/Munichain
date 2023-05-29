using Data.DatabaseServices;
using Microsoft.EntityFrameworkCore;

namespace Tests.TestStartup;

public class TestDbContextFactory : IDbContextFactory<SqlDbContext>
{
    private DbContextOptions<SqlDbContext> _options;

    public TestDbContextFactory(string databaseName = "InMemoryTest")
    {
        _options = new DbContextOptionsBuilder<SqlDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public SqlDbContext CreateDbContext()
    {
        return new SqlDbContext(_options);
    }
}