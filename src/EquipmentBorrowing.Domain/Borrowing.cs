namespace EquipmentBorrowing.Domain;

public sealed class Borrowing
{
    public Borrowing(int id, int studentId, int equipmentId, DateOnly dateBorrowed, DateOnly expectedReturnDate)
    {
        if (expectedReturnDate <= dateBorrowed)
            throw new ArgumentException("Expected return date must be later than the checkout date.", nameof(expectedReturnDate));
        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public int Id { get; private set; }
    public int StudentId { get; private set; }
    public int EquipmentId { get; private set; }
    public DateOnly DateBorrowed { get; private set; }
    public DateOnly ExpectedReturnDate { get; private set; }
    public DateOnly? DateReturned { get; private set; }
    public BorrowingStatus Status { get; private set; }

    public void MarkReturned(DateOnly dateReturned)
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException("Borrowing has already been returned.");
        if (dateReturned < DateBorrowed)
            throw new ArgumentException("Return date cannot be earlier than checkout date.", nameof(dateReturned));
        DateReturned = dateReturned;
        Status = BorrowingStatus.Returned;
    }
}
