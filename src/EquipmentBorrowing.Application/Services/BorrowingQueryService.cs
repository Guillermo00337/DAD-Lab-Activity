using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public sealed class BorrowingQueryService
{
    private readonly IBorrowingRepository _repository;
    public BorrowingQueryService(IBorrowingRepository repository) => _repository = repository;

    public Task<IReadOnlyList<BorrowingHistoryItem>?> GetStudentHistoryAsync(int studentId, CancellationToken ct = default) =>
        _repository.GetStudentHistoryAsync(studentId, ct);

    public Task<IReadOnlyList<BorrowingHistoryItem>?> GetEquipmentHistoryAsync(int equipmentId, CancellationToken ct = default) =>
        _repository.GetEquipmentHistoryAsync(equipmentId, ct);

    public Task<IReadOnlyList<BorrowingHistoryItem>> SearchAsync(BorrowingSearchCriteria criteria, CancellationToken ct = default) =>
        _repository.SearchAsync(criteria, ct);
}
