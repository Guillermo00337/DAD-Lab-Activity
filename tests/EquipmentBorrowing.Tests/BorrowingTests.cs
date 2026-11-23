using EquipmentBorrowing.Domain;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowingTests
{
    [Fact]
    public void Constructor_ValidDates_CreatesActiveBorrowing()
    {
        var borrowing = new Borrowing(
            1, 1, 1,
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        Assert.Equal(BorrowingStatus.Active, borrowing.Status);
        Assert.Null(borrowing.DateReturned);
    }

    [Fact]
    public void Constructor_ExpectedReturnBeforeOrEqualToCheckout_ThrowsException()
    {
        var checkoutDate = new DateOnly(2026, 11, 20);

        Assert.Throws<ArgumentException>(() =>
            new Borrowing(1, 1, 1, checkoutDate, checkoutDate));
    }

    [Fact]
    public void MarkReturned_ValidDate_ChangesStatusToReturned()
    {
        var borrowing = new Borrowing(
            1, 1, 1,
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        var returnDate = new DateOnly(2026, 11, 25);
        borrowing.MarkReturned(returnDate);

        Assert.Equal(BorrowingStatus.Returned, borrowing.Status);
        Assert.Equal(returnDate, borrowing.DateReturned);
    }

    [Fact]
    public void MarkReturned_DateBeforeCheckout_ThrowsException()
    {
        var borrowing = new Borrowing(
            1, 1, 1,
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        Assert.Throws<ArgumentException>(() =>
            borrowing.MarkReturned(new DateOnly(2026, 11, 19)));
    }

    [Fact]
    public void MarkReturned_AlreadyReturned_ThrowsException()
    {
        var borrowing = new Borrowing(
            1, 1, 1,
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 11, 27));

        borrowing.MarkReturned(new DateOnly(2026, 11, 25));

        Assert.Throws<InvalidOperationException>(() =>
            borrowing.MarkReturned(new DateOnly(2026, 11, 26)));
    }
}
