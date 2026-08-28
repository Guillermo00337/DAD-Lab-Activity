namespace EquipmentBorrowing.Domain;

public sealed class Student
{
    public Student(
        int id,
        string studentNumber,
        string fullName,
        bool isAllowedToBorrow,
        int maxActiveBorrowings)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new ArgumentException("Student number is required.", nameof(studentNumber));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        if (maxActiveBorrowings < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxActiveBorrowings), "Maximum active borrowings cannot be negative.");
        }

        Id = id;
        StudentNumber = studentNumber;
        FullName = fullName;
        IsAllowedToBorrow = isAllowedToBorrow;
        MaxActiveBorrowings = maxActiveBorrowings;
    }

    public int Id { get; }

    public string StudentNumber { get; }

    public string FullName { get; }

    public bool IsAllowedToBorrow { get; private set; }

    public int MaxActiveBorrowings { get; }

    public bool CanStartBorrowing(int activeBorrowingCount) =>
        IsAllowedToBorrow && activeBorrowingCount < MaxActiveBorrowings;

    public void BlockBorrowing() => IsAllowedToBorrow = false;

    public void AllowBorrowing() => IsAllowedToBorrow = true;
}
