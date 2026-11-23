using EquipmentBorrowing.Domain;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class StudentTests
{
    [Fact]
    public void CanStartBorrowing_EligibleStudentBelowLimit_ReturnsTrue()
    {
        var student = new Student(1, "2026-001", "Lia Cruz");

        Assert.True(student.CanStartBorrowing(0));
    }

    [Fact]
    public void CanStartBorrowing_BlockedStudent_ReturnsFalse()
    {
        var student = new Student(1, "2026-001", "Lia Cruz");
        student.BlockBorrowing();

        Assert.False(student.CanStartBorrowing(0));
    }

    [Fact]
    public void CanStartBorrowing_AtPersonalLimit_ReturnsFalse()
    {
        var student = new Student(1, "2026-001", "Lia Cruz",
            maxActiveBorrowings: 2);

        Assert.False(student.CanStartBorrowing(2));
    }

    [Fact]
    public void CanStartBorrowing_AtSystemLimit_ReturnsFalse()
    {
        var student = new Student(1, "2026-001", "Lia Cruz");

        Assert.False(student.CanStartBorrowing(3));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Constructor_InvalidBorrowingLimit_ThrowsException(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Student(1, "2026-001", "Lia Cruz",
                maxActiveBorrowings: limit));
    }

    [Fact]
    public void UpdateDetails_TrimsStudentInformation()
    {
        var student = new Student(1, "2026-001", "Lia Cruz");

        student.UpdateDetails(" 2026-002 ", " Ana Santos ", "  09123456789  ");

        Assert.Equal("2026-002", student.StudentNumber);
        Assert.Equal("Ana Santos", student.FullName);
        Assert.Equal("09123456789", student.ContactInformation);
    }
}
