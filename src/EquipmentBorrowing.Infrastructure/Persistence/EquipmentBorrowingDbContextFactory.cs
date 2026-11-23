using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class EquipmentBorrowingDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        string? connectionString = Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set EQUIPMENT_BORROWING_CONNECTION_STRING before running EF Core migration commands.");
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>().UseNpgsql(connectionString).Options;
        return new EquipmentBorrowingDbContext(options);
    }
}
