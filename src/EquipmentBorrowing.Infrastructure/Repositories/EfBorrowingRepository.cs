using System.Data;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfBorrowingRepository : IBorrowingRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;
    public EfBorrowingRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory) => _factory = factory;

    public async Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.CountAsync(x => x.StudentId == studentId && x.Status == BorrowingStatus.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.AsNoTracking().Where(x => x.Status == BorrowingStatus.Active)
            .OrderBy(x => x.ExpectedReturnDate).ThenBy(x => x.Id).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await (from b in db.Borrowings.AsNoTracking()
                      join s in db.Students.AsNoTracking() on b.StudentId equals s.Id
                      join e in db.Equipment.AsNoTracking() on b.EquipmentId equals e.Id
                      where b.Status == BorrowingStatus.Active
                      orderby b.ExpectedReturnDate, b.Id
                      select new ActiveBorrowingDetails(b.Id, s.FullName, e.Name, b.DateBorrowed, b.ExpectedReturnDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Borrowings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CheckoutResult> CheckoutAsync(
        int studentId, IReadOnlyCollection<int> equipmentIds, DateOnly checkoutDate,
        DateOnly expectedReturnDate, bool requireMultiple, CancellationToken cancellationToken = default)
    {
        if (equipmentIds is null || equipmentIds.Count == 0)
            return CheckoutResult.Failure("Select at least one equipment item.");
        if (requireMultiple && equipmentIds.Count < 2)
            return CheckoutResult.Failure("Multi-item checkout requires at least two items.");
        if (equipmentIds.Distinct().Count() != equipmentIds.Count)
            return CheckoutResult.Failure("Duplicate equipment selections are not allowed.");
        if (expectedReturnDate <= checkoutDate)
            return CheckoutResult.Failure("Expected return date must be later than checkout date.");

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var student = await db.Students.SingleOrDefaultAsync(x => x.Id == studentId, cancellationToken);
            if (student is null) return await RollbackFailure(transaction, "Student no longer exists.", cancellationToken);
            if (!student.IsAllowedToBorrow) return await RollbackFailure(transaction, "Student is not eligible to borrow new equipment.", cancellationToken);

            int currentCount = await db.Borrowings.CountAsync(
                x => x.StudentId == studentId && x.Status == BorrowingStatus.Active, cancellationToken);
            if (currentCount + equipmentIds.Count > Math.Min(3, student.MaxActiveBorrowings))
                return await RollbackFailure(transaction, "The checkout would exceed the student's limit of three active items.", cancellationToken);

            var uniqueIds = equipmentIds.Distinct().ToArray();
            var equipment = await db.Equipment.Where(x => uniqueIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (equipment.Count != uniqueIds.Length)
                return await RollbackFailure(transaction, "One or more selected equipment records no longer exist.", cancellationToken);

            var idsWithActiveLoans = await db.Borrowings
                .Where(x => uniqueIds.Contains(x.EquipmentId) && x.Status == BorrowingStatus.Active)
                .Select(x => x.EquipmentId).ToListAsync(cancellationToken);
            foreach (var item in equipment)
            {
                if (!item.IsAvailable || idsWithActiveLoans.Contains(item.Id))
                    return await RollbackFailure(transaction, $"{item.AssetTag} — {item.Name} is no longer available.", cancellationToken);
            }

            var records = new List<Borrowing>(equipment.Count);
            bool injectTestFailure = requireMultiple &&
                string.Equals(Environment.GetEnvironmentVariable("EQUIPMENT_BORROWING_TEST_FAIL_AFTER_FIRST_INSERT"), "true", StringComparison.OrdinalIgnoreCase);
            int persistedWithinTransaction = 0;
            foreach (var item in equipment)
            {
                item.MarkBorrowed();
                var record = new Borrowing(0, studentId, item.Id, checkoutDate, expectedReturnDate);
                records.Add(record);
                db.Borrowings.Add(record);
                // Each SaveChanges remains inside the same serializable transaction.
                // This makes the optional failure seam capable of proving rollback after a real write.
                await db.SaveChangesAsync(cancellationToken);
                persistedWithinTransaction++;
                if (injectTestFailure && persistedWithinTransaction == 1)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return CheckoutResult.Failure("TEST ONLY: deliberate failure after the first item was written inside the transaction. The entire checkout was rolled back; verify with a fresh database connection.");
                }
            }

            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch (PostgresException)
            {
                // PostgreSQL returned a definite server-side error (for example, a serialization failure).
                return CheckoutResult.Failure("PostgreSQL rejected the checkout transaction. No success was reported; refresh the records and try again.");
            }
            catch (NpgsqlException)
            {
                // A transport failure during COMMIT can make the acknowledgement unknown. Never retry automatically.
                return CheckoutResult.Failure("The checkout commit outcome is unknown because the database connection was lost. Reconnect and refresh borrowing and equipment records before deciding whether to retry.");
            }
            catch (TimeoutException)
            {
                return CheckoutResult.Failure("The checkout commit outcome is unknown because the database timed out. Reconnect and refresh records before deciding whether to retry.");
            }

            return CheckoutResult.Success(records);
        }
        catch (DbUpdateException)
        {
            await SafeRollback(transaction, cancellationToken);
            return CheckoutResult.Failure("PostgreSQL rejected the checkout because a database constraint or concurrent update conflicted. No success was reported; refresh and try again.");
        }
        catch (PostgresException)
        {
            await SafeRollback(transaction, cancellationToken);
            return CheckoutResult.Failure("PostgreSQL rejected the checkout because the current data conflicted with a database rule. Refresh and try again.");
        }
        catch (NpgsqlException)
        {
            await SafeRollback(transaction, cancellationToken);
            return CheckoutResult.Failure("The database operation failed before the commit was acknowledged. Refresh the records before retrying.");
        }
    }

    public async Task<EntityOperationResult> ReturnAsync(int borrowingId, DateOnly returnDate, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var borrowing = await db.Borrowings.SingleOrDefaultAsync(x => x.Id == borrowingId, cancellationToken);
            if (borrowing is null) return await RollbackEntityFailure(transaction, "Borrowing record no longer exists.", cancellationToken);
            if (borrowing.Status != BorrowingStatus.Active) return await RollbackEntityFailure(transaction, "This borrowing has already been returned.", cancellationToken);
            if (returnDate < borrowing.DateBorrowed) return await RollbackEntityFailure(transaction, "Return date cannot be earlier than checkout date.", cancellationToken);

            var equipment = await db.Equipment.SingleOrDefaultAsync(x => x.Id == borrowing.EquipmentId, cancellationToken);
            if (equipment is null) return await RollbackEntityFailure(transaction, "The linked equipment record no longer exists.", cancellationToken);

            borrowing.MarkReturned(returnDate);
            equipment.MarkAvailable();
            await db.SaveChangesAsync(cancellationToken);
            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch (PostgresException)
            {
                return EntityOperationResult.Failure("PostgreSQL rejected the return transaction. No success was reported; refresh the records before retrying.");
            }
            catch (NpgsqlException)
            {
                return EntityOperationResult.Failure("The return commit outcome is unknown because the database connection was lost. Reconnect and refresh before deciding whether to retry.");
            }
            catch (TimeoutException)
            {
                return EntityOperationResult.Failure("The return commit outcome is unknown because the database timed out. Reconnect and refresh before deciding whether to retry.");
            }
            return EntityOperationResult.Success("Equipment returned successfully. The borrowing history was retained.");
        }
        catch (DbUpdateException)
        {
            await SafeRollback(transaction, cancellationToken);
            return EntityOperationResult.Failure("The return could not be saved because a database constraint or concurrent update conflicted. Refresh and try again.");
        }
        catch (NpgsqlException)
        {
            await SafeRollback(transaction, cancellationToken);
            return EntityOperationResult.Failure("The database operation failed. Refresh the borrowing and equipment records before retrying.");
        }
    }

    public async Task<IReadOnlyList<BorrowingHistoryItem>?> GetStudentHistoryAsync(
    int studentId,
    CancellationToken cancellationToken = default)
{
    await using var db = await _factory.CreateDbContextAsync(cancellationToken);

    bool exists = await db.Students.AnyAsync(
        x => x.Id == studentId,
        cancellationToken);

    if (!exists) return null;

    var query =
        from b in db.Borrowings.AsNoTracking()
        join s in db.Students.AsNoTracking()
            on b.StudentId equals s.Id
        join e in db.Equipment.AsNoTracking()
            on b.EquipmentId equals e.Id
        where s.Id == studentId
        orderby b.DateBorrowed descending, b.Id
        select new BorrowingHistoryItem(
            b.Id,
            s.Id,
            s.StudentNumber,
            s.FullName,
            e.Id,
            e.AssetTag,
            e.Name,
            b.DateBorrowed,
            b.ExpectedReturnDate,
            b.DateReturned,
            b.Status);

    return await query.ToListAsync(cancellationToken);
}

public async Task<IReadOnlyList<BorrowingHistoryItem>?> GetEquipmentHistoryAsync(
    int equipmentId,
    CancellationToken cancellationToken = default)
{
    await using var db = await _factory.CreateDbContextAsync(cancellationToken);

    bool exists = await db.Equipment.AnyAsync(
        x => x.Id == equipmentId,
        cancellationToken);

    if (!exists) return null;

    var query =
        from b in db.Borrowings.AsNoTracking()
        join s in db.Students.AsNoTracking()
            on b.StudentId equals s.Id
        join e in db.Equipment.AsNoTracking()
            on b.EquipmentId equals e.Id
        where e.Id == equipmentId
        orderby b.DateBorrowed descending, b.Id
        select new BorrowingHistoryItem(
            b.Id,
            s.Id,
            s.StudentNumber,
            s.FullName,
            e.Id,
            e.AssetTag,
            e.Name,
            b.DateBorrowed,
            b.ExpectedReturnDate,
            b.DateReturned,
            b.Status);

    return await query.ToListAsync(cancellationToken);
}

    public async Task<IReadOnlyList<BorrowingHistoryItem>> SearchAsync(
    BorrowingSearchCriteria criteria,
    CancellationToken cancellationToken = default)
{
    await using var db = await _factory.CreateDbContextAsync(cancellationToken);

    // Join the database tables first, keeping the query SQL-translatable.
    var query =
        from b in db.Borrowings.AsNoTracking()
        join s in db.Students.AsNoTracking()
            on b.StudentId equals s.Id
        join e in db.Equipment.AsNoTracking()
            on b.EquipmentId equals e.Id
        select new
        {
            BorrowingId = b.Id,
            StudentId = s.Id,
            StudentNumber = s.StudentNumber,
            StudentName = s.FullName,
            EquipmentId = e.Id,
            AssetTag = e.AssetTag,
            EquipmentName = e.Name,
            DateBorrowed = b.DateBorrowed,
            ExpectedReturnDate = b.ExpectedReturnDate,
            DateReturned = b.DateReturned,
            Status = b.Status
        };

    // Apply status filtering before creating the history objects.
    if (criteria.Status == BorrowingStatusFilter.Active)
        query = query.Where(x => x.Status == BorrowingStatus.Active);

    if (criteria.Status == BorrowingStatusFilter.Returned)
        query = query.Where(x => x.Status == BorrowingStatus.Returned);

    // Search by student name, student number, or equipment name.
    string term = criteria.SearchText?.Trim() ?? string.Empty;

    if (term.Length > 0)
    {
        string pattern = $"%{EscapeLike(term)}%";

        query = query.Where(x =>
            EF.Functions.ILike(x.StudentName, pattern, "\\") ||
            EF.Functions.ILike(x.StudentNumber, pattern, "\\") ||
            EF.Functions.ILike(x.EquipmentName, pattern, "\\"));
    }

    // Apply sorting before the final projection.
    if (criteria.SortBy == BorrowingSortField.ExpectedReturnDate)
    {
        query = criteria.Descending
            ? query.OrderByDescending(x => x.ExpectedReturnDate)
                .ThenBy(x => x.BorrowingId)
            : query.OrderBy(x => x.ExpectedReturnDate)
                .ThenBy(x => x.BorrowingId);
    }
    else
    {
        query = criteria.Descending
            ? query.OrderByDescending(x => x.DateBorrowed)
                .ThenBy(x => x.BorrowingId)
            : query.OrderBy(x => x.DateBorrowed)
                .ThenBy(x => x.BorrowingId);
    }

    // Create BorrowingHistoryItem only after filtering and sorting.
    return await query
        .Select(x => new BorrowingHistoryItem(
            x.BorrowingId,
            x.StudentId,
            x.StudentNumber,
            x.StudentName,
            x.EquipmentId,
            x.AssetTag,
            x.EquipmentName,
            x.DateBorrowed,
            x.ExpectedReturnDate,
            x.DateReturned,
            x.Status))
        .ToListAsync(cancellationToken);
}

    private static IQueryable<BorrowingHistoryItem> HistoryQuery(EquipmentBorrowingDbContext db) =>
        from b in db.Borrowings.AsNoTracking()
        join s in db.Students.AsNoTracking() on b.StudentId equals s.Id
        join e in db.Equipment.AsNoTracking() on b.EquipmentId equals e.Id
        select new BorrowingHistoryItem(b.Id, s.Id, s.StudentNumber, s.FullName, e.Id, e.AssetTag,
            e.Name, b.DateBorrowed, b.ExpectedReturnDate, b.DateReturned, b.Status);

    private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static async Task<CheckoutResult> RollbackFailure(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, string message, CancellationToken ct)
    {
        await tx.RollbackAsync(ct);
        return CheckoutResult.Failure(message);
    }

    private static async Task<EntityOperationResult> RollbackEntityFailure(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, string message, CancellationToken ct)
    {
        await tx.RollbackAsync(ct);
        return EntityOperationResult.Failure(message);
    }

    private static async Task SafeRollback(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, CancellationToken ct)
    {
        try { await tx.RollbackAsync(ct); } catch { /* Preserve the original failure. */ }
    }
}
