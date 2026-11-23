namespace EquipmentBorrowing.Domain;

public sealed class Student
{
    public Student(
        int id,
        string studentNumber,
        string fullName,
        bool isAllowedToBorrow = true,
        int maxActiveBorrowings = 3,
        string? contactInformation = null)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Student number is required.", nameof(studentNumber));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (maxActiveBorrowings is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(maxActiveBorrowings), "The maximum active borrowing limit must be between zero and three.");

        Id = id;
        StudentNumber = studentNumber.Trim();
        FullName = fullName.Trim();
        IsAllowedToBorrow = isAllowedToBorrow;
        MaxActiveBorrowings = maxActiveBorrowings;
        ContactInformation = NormalizeOptional(contactInformation);
    }

    public int Id { get; private set; }
    public string StudentNumber { get; private set; }
    public string FullName { get; private set; }
    public string? ContactInformation { get; private set; }
    public bool IsAllowedToBorrow { get; private set; }
    public int MaxActiveBorrowings { get; private set; }

    public bool CanStartBorrowing(int activeBorrowingCount) =>
        IsAllowedToBorrow && activeBorrowingCount < MaxActiveBorrowings && activeBorrowingCount < 3;

    public void UpdateDetails(string studentNumber, string fullName, string? contactInformation)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            throw new ArgumentException("Student number is required.", nameof(studentNumber));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));

        StudentNumber = studentNumber.Trim();
        FullName = fullName.Trim();
        ContactInformation = NormalizeOptional(contactInformation);
    }

    public void SetBorrowingEligibility(bool allowed) => IsAllowedToBorrow = allowed;
    public void BlockBorrowing() => IsAllowedToBorrow = false;
    public void AllowBorrowing() => IsAllowedToBorrow = true;

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}
