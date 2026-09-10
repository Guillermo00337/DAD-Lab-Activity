namespace EquipmentBorrowing.Application.Services;

public sealed record ActiveBorrowingDetails(
    int BorrowingId,
    string StudentName,
    string EquipmentName,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);
