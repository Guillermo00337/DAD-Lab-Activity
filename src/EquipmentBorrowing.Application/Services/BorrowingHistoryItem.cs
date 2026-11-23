using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record BorrowingHistoryItem(
    int BorrowingId,
    int StudentId,
    string StudentNumber,
    string StudentName,
    int EquipmentId,
    string EquipmentIdentifier,
    string EquipmentName,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate,
    DateOnly? DateReturned,
    BorrowingStatus Status);
