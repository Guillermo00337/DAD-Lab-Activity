using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record CheckoutResult(bool Succeeded, string Message, IReadOnlyList<Borrowing> Borrowings)
{
    public static CheckoutResult Success(IReadOnlyList<Borrowing> borrowings) =>
        new(true, borrowings.Count == 1 ? "Equipment borrowed successfully." : $"{borrowings.Count} items checked out successfully.", borrowings);

    public static CheckoutResult Failure(string message) => new(false, message, Array.Empty<Borrowing>());
}
