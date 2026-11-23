using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public sealed class ReturnEquipmentService
{
    private readonly IBorrowingRepository _repository;
    public ReturnEquipmentService(IBorrowingRepository repository) => _repository = repository;

    public async Task<ReturnEquipmentResult> ReturnAsync(ReturnEquipmentRequest request, CancellationToken cancellationToken = default)
    {
        EntityOperationResult result = await _repository.ReturnAsync(request.BorrowingId, request.DateReturned, cancellationToken);
        return result.Succeeded ? ReturnEquipmentResult.Success(result.Message) : ReturnEquipmentResult.Failure(result.Message);
    }
}
