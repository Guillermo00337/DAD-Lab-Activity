using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace EquipmentBorrowing.Tests;

public sealed class PostgresTestFixture : IAsyncLifetime
{
    public EquipmentBorrowingDbContextFactoryForTests ContextFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set EQUIPMENT_BORROWING_TEST_CONNECTION_STRING before running PostgreSQL integration tests.");
        }

        var connection = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.Equals(
            connection.Database,
            "equipment_borrowing_test",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SAFETY CHECK FAILED: Integration tests may only use the equipment_borrowing_test database.");
        }

        ContextFactory = new EquipmentBorrowingDbContextFactoryForTests(connectionString);

        await using var db = await ContextFactory.CreateDbContextAsync();

        // Apply migrations only. Never delete or recreate the database.
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public sealed class EquipmentBorrowingDbContextFactoryForTests
    : IDbContextFactory<EquipmentBorrowingDbContext>
{
    private readonly DbContextOptions<EquipmentBorrowingDbContext> _options;

    public EquipmentBorrowingDbContextFactoryForTests(string connectionString)
    {
        _options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    public EquipmentBorrowingDbContext CreateDbContext()
    {
        return new EquipmentBorrowingDbContext(_options);
    }

    public Task<EquipmentBorrowingDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
