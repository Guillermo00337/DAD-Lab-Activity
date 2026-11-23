using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveDetailsAsync(CancellationToken cancellationToken = default);
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CheckoutResult> CheckoutAsync(int studentId, IReadOnlyCollection<int> equipmentIds, DateOnly checkoutDate, DateOnly expectedReturnDate, bool requireMultiple, CancellationToken cancellationToken = default);
    Task<EntityOperationResult> ReturnAsync(int borrowingId, DateOnly returnDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BorrowingHistoryItem>?> GetStudentHistoryAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BorrowingHistoryItem>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BorrowingHistoryItem>> SearchAsync(BorrowingSearchCriteria criteria, CancellationToken cancellationToken = default);
}
