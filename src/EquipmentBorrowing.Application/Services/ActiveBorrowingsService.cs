using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public sealed class ActiveBorrowingsService
{
    private readonly IBorrowingRepository _borrowingRepository;
    public ActiveBorrowingsService(IBorrowingRepository borrowingRepository)
    {
        _borrowingRepository = borrowingRepository;
    }

    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveBorrowingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _borrowingRepository.GetActiveDetailsAsync(cancellationToken);
    }
}
