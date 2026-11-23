using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class BorrowEquipmentService
{
    private readonly IBorrowingRepository _repository;
    public BorrowEquipmentService(IBorrowingRepository repository) => _repository = repository;

    public async Task<BorrowEquipmentResult> BorrowAsync(BorrowEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        CheckoutResult result = await _repository.CheckoutAsync(
            request.StudentId, new[] { request.EquipmentId }, request.DateBorrowed,
            request.ExpectedReturnDate, requireMultiple: false, cancellationToken);
        return result.Succeeded
            ? BorrowEquipmentResult.Success(result.Borrowings[0])
            : BorrowEquipmentResult.Failure(result.Message);
    }

    public Task<CheckoutResult> BorrowMultipleAsync(
        int studentId, IReadOnlyCollection<int> equipmentIds, DateOnly checkoutDate,
        DateOnly expectedReturnDate, CancellationToken cancellationToken = default) =>
        _repository.CheckoutAsync(studentId, equipmentIds, checkoutDate, expectedReturnDate,
            requireMultiple: true, cancellationToken);
}
