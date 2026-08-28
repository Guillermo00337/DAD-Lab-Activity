using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record BorrowEquipmentResult(
    bool Succeeded,
    string Message,
    Borrowing? Borrowing)
{
    public static BorrowEquipmentResult Success(Borrowing borrowing) =>
        new(true, "Borrowing approved.", borrowing);

    public static BorrowEquipmentResult Failure(string message) =>
        new(false, message, null);
}
