using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _dbContextFactory;

    public EfBorrowingRepository(IDbContextFactory<EquipmentBorrowingDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Borrowings.CountAsync(
            borrowing => borrowing.StudentId == studentId && borrowing.Status == BorrowingStatus.Active,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Borrowings
            .AsNoTracking()
            .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
            .OrderBy(borrowing => borrowing.ExpectedReturnDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await (
            from borrowing in dbContext.Borrowings.AsNoTracking()
            join student in dbContext.Students.AsNoTracking() on borrowing.StudentId equals student.Id
            join equipment in dbContext.Equipment.AsNoTracking() on borrowing.EquipmentId equals equipment.Id
            where borrowing.Status == BorrowingStatus.Active
            orderby borrowing.ExpectedReturnDate
            select new ActiveBorrowingDetails(
                borrowing.Id,
                student.FullName,
                equipment.Name,
                borrowing.DateBorrowed,
                borrowing.ExpectedReturnDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Borrowings
            .SingleOrDefaultAsync(borrowing => borrowing.Id == id, cancellationToken);
    }

    public async Task<Borrowing> AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        dbContext.Borrowings.Add(borrowing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return borrowing;
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using EquipmentBorrowingDbContext dbContext =
            await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        dbContext.Borrowings.Update(borrowing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
