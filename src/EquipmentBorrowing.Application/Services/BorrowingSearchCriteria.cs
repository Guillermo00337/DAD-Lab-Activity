using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public enum BorrowingStatusFilter { All, Active, Returned }
public enum BorrowingSortField { CheckoutDate, ExpectedReturnDate }

public sealed record BorrowingSearchCriteria(
    string? SearchText = null,
    BorrowingStatusFilter Status = BorrowingStatusFilter.All,
    BorrowingSortField SortBy = BorrowingSortField.CheckoutDate,
    bool Descending = true);
