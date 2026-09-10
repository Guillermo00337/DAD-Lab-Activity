using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = [];
    private int _nextId = 1;

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        int count = _borrowings.Count(
            borrowing => borrowing.StudentId == studentId &&
                         borrowing.Status == BorrowingStatus.Active);

        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Borrowing> activeBorrowings = _borrowings
            .Where(borrowing => borrowing.Status == BorrowingStatus.Active)
            .ToList();

        return Task.FromResult(activeBorrowings);
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Borrowing? borrowing = _borrowings.SingleOrDefault(item => item.Id == id);
        return Task.FromResult(borrowing);
    }

    public Task<Borrowing> AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        var storedBorrowing = borrowing.Id == 0
            ? new Borrowing(
                _nextId++,
                borrowing.StudentId,
                borrowing.EquipmentId,
                borrowing.DateBorrowed,
                borrowing.ExpectedReturnDate)
            : borrowing;

        _borrowings.Add(storedBorrowing);
        return Task.FromResult(storedBorrowing);
    }

    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
