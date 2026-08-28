using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var students = new[]
{
    new Student(1, "2026-0001", "Ana Reyes", isAllowedToBorrow: true, maxActiveBorrowings: 2),
    new Student(2, "2026-0002", "Marco Santos", isAllowedToBorrow: false, maxActiveBorrowings: 2)
};

var equipment = new[]
{
    new Equipment(1, "CAM-001", "Digital Camera"),
    new Equipment(2, "MIC-001", "Wireless Microphone")
};

var studentRepository = new InMemoryStudentRepository(students);
var equipmentRepository = new InMemoryEquipmentRepository(equipment);
var borrowingRepository = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository);

DateOnly today = DateOnly.FromDateTime(DateTime.Today);

BorrowEquipmentResult successfulRequest = await service.BorrowAsync(
    new BorrowEquipmentRequest(
        StudentId: 1,
        EquipmentId: 1,
        DateBorrowed: today,
        ExpectedReturnDate: today.AddDays(7)));

PrintResult("Successful case", successfulRequest);

BorrowEquipmentResult failedRequest = await service.BorrowAsync(
    new BorrowEquipmentRequest(
        StudentId: 2,
        EquipmentId: 2,
        DateBorrowed: today,
        ExpectedReturnDate: today.AddDays(7)));

PrintResult("Failure case", failedRequest);

static void PrintResult(string title, BorrowEquipmentResult result)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
    Console.WriteLine(result.Succeeded ? "Status: Success" : "Status: Failed");
    Console.WriteLine($"Message: {result.Message}");

    if (result.Borrowing is not null)
    {
        Console.WriteLine($"Borrowing Status: {result.Borrowing.Status}");
        Console.WriteLine($"Student Id: {result.Borrowing.StudentId}");
        Console.WriteLine($"Equipment Id: {result.Borrowing.EquipmentId}");
        Console.WriteLine($"Expected Return: {result.Borrowing.ExpectedReturnDate}");
    }

    Console.WriteLine();
}
