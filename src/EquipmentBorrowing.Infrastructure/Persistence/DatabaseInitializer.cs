using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;
    public DatabaseInitializer(IDbContextFactory<EquipmentBorrowingDbContext> factory) => _factory = factory;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);

        if (!await db.Students.AnyAsync(cancellationToken))
        {
            db.Students.AddRange(
                new Student(0, "2026-0001", "Ana Reyes", true, 3, "ana.reyes@example.edu"),
                new Student(0, "2026-0002", "Marco Santos", false, 3, null),
                new Student(0, "2026-0003", "Lia Cruz", true, 3, null));
        }

        if (!await db.Equipment.AnyAsync(cancellationToken))
        {
            db.Equipment.AddRange(
                new Equipment(0, "CAM-001", "Digital Camera", true, "Mirrorless camera"),
                new Equipment(0, "MIC-001", "Wireless Microphone", true, "Wireless audio microphone"),
                new Equipment(0, "TAB-001", "Drawing Tablet", true, "Pen-enabled graphics tablet"),
                new Equipment(0, "LAP-001", "Laptop", true, "Laboratory laptop"));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
