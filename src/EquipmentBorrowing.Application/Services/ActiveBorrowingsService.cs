using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Application.Services;

public sealed class ActiveBorrowingsService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ActiveBorrowingsService(
        IBorrowingRepository borrowingRepository,
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveBorrowingsAsync(
        CancellationToken cancellationToken = default)
    {
        var details = new List<ActiveBorrowingDetails>();
        IReadOnlyList<Domain.Borrowing> borrowings = await _borrowingRepository.GetActiveAsync(cancellationToken);

        foreach (Domain.Borrowing borrowing in borrowings)
        {
            Domain.Student? student = await _studentRepository.GetByIdAsync(borrowing.StudentId, cancellationToken);
            Domain.Equipment? equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);

            details.Add(new ActiveBorrowingDetails(
                borrowing.Id,
                student?.FullName ?? "Unknown student",
                equipment?.Name ?? "Unknown equipment",
                borrowing.DateBorrowed,
                borrowing.ExpectedReturnDate));
        }

        return details;
    }
}
