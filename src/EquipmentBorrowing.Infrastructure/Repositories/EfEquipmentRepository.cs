using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfEquipmentRepository : IEquipmentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;
    public EfEquipmentRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory) => _factory = factory;

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Equipment.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Equipment.AsNoTracking().OrderBy(x => x.AssetTag).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Equipment.AsNoTracking().Where(x => x.IsAvailable)
            .OrderBy(x => x.AssetTag).ToListAsync(cancellationToken);
    }

    public async Task<Equipment> AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Equipment.Add(equipment);
        await db.SaveChangesAsync(cancellationToken);
        return equipment;
    }

    public async Task UpdateDetailsAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var current = await db.Equipment.SingleOrDefaultAsync(x => x.Id == equipment.Id, cancellationToken)
            ?? throw new InvalidOperationException("Equipment no longer exists.");
        // Copy only descriptive fields so availability remains owned by borrow/return workflows.
        current.UpdateDetails(equipment.AssetTag, equipment.Name, equipment.Description);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IdentifierExistsAsync(string identifier, int? exceptId = null, CancellationToken cancellationToken = default)
    {
        string normalized = identifier.Trim().ToUpperInvariant();
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Equipment.AnyAsync(x => x.Id != (exceptId ?? 0) && x.AssetTag.Trim().ToUpper() == normalized, cancellationToken);
    }
}
