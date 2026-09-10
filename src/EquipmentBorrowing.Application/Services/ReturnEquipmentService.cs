using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnEquipmentResult> ReturnAsync(
        ReturnEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        Borrowing? borrowing = await _borrowingRepository.GetByIdAsync(request.BorrowingId, cancellationToken);
        if (borrowing is null)
        {
            return ReturnEquipmentResult.Failure("Borrowing record does not exist.");
        }

        if (borrowing.Status == BorrowingStatus.Returned)
        {
            return ReturnEquipmentResult.Failure("Borrowing has already been returned.");
        }

        Equipment? equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
        if (equipment is null)
        {
            return ReturnEquipmentResult.Failure("Borrowed equipment record does not exist.");
        }

        borrowing.MarkReturned(request.DateReturned);
        equipment.MarkAvailable();

        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ReturnEquipmentResult.Success();
    }
}
