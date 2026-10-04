using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _dbContextFactory;

    public DatabaseInitializer(IDbContextFactory<EquipmentBorrowingDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Students.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Students.AddRange(
            new Student(1, "2026-0001", "Ana Reyes", isAllowedToBorrow: true, maxActiveBorrowings: 2),
            new Student(2, "2026-0002", "Marco Santos", isAllowedToBorrow: false, maxActiveBorrowings: 2),
            new Student(3, "2026-0003", "Lia Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 1));

        dbContext.Equipment.AddRange(
            new Equipment(1, "CAM-001", "Digital Camera"),
            new Equipment(2, "MIC-001", "Wireless Microphone"),
            new Equipment(3, "TAB-001", "Drawing Tablet"),
            new Equipment(4, "LAP-001", "Laptop"));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
