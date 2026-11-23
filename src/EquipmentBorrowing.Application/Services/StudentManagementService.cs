using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record RegisterStudentRequest(string StudentNumber, string FullName, string? ContactInformation);
public sealed record UpdateStudentRequest(int StudentId, string StudentNumber, string FullName, string? ContactInformation);

public sealed class StudentManagementService
{
    private readonly IStudentRepository _repository;
    public StudentManagementService(IStudentRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken ct = default) => _repository.GetAllAsync(ct);

    public async Task<EntityOperationResult> RegisterAsync(RegisterStudentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.StudentNumber)) return EntityOperationResult.Failure("Student number is required.");
        if (string.IsNullOrWhiteSpace(request.FullName)) return EntityOperationResult.Failure("Full name is required.");
        if (!string.IsNullOrEmpty(request.ContactInformation) && string.IsNullOrWhiteSpace(request.ContactInformation))
            return EntityOperationResult.Failure("Optional contact information must be omitted or contain nonblank text.");
        try
        {
            string number = request.StudentNumber.Trim();
            if (await _repository.NumberExistsAsync(number, cancellationToken: ct))
                return EntityOperationResult.Failure("That student number is already registered (student numbers are case-insensitive).");
            await _repository.AddAsync(new Student(0, number, request.FullName, true, 3, request.ContactInformation), ct);
            return EntityOperationResult.Success("Student registered and eligible to borrow.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return EntityOperationResult.Failure("The student could not be saved because of a database or uniqueness conflict. Check the connection, refresh the list and try again.");
        }
    }

    public async Task<EntityOperationResult> UpdateAsync(UpdateStudentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.StudentNumber)) return EntityOperationResult.Failure("Student number is required.");
        if (string.IsNullOrWhiteSpace(request.FullName)) return EntityOperationResult.Failure("Full name is required.");
        try
        {
            Student? student = await _repository.GetByIdAsync(request.StudentId, ct);
            if (student is null) return EntityOperationResult.Failure("Student no longer exists. Refresh the list.");
            if (await _repository.NumberExistsAsync(request.StudentNumber.Trim(), request.StudentId, ct))
                return EntityOperationResult.Failure("Another student already uses that student number.");
            student.UpdateDetails(request.StudentNumber, request.FullName, request.ContactInformation);
            await _repository.UpdateDetailsAsync(student, ct);
            return EntityOperationResult.Success("Student information updated. Borrowing history remains linked to the same student.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return EntityOperationResult.Failure("The student update could not be saved because of a database or uniqueness conflict. Refresh and try again.");
        }
    }

    public async Task<EntityOperationResult> SetEligibilityAsync(int studentId, bool allowed, CancellationToken ct = default)
    {
        try
        {
            Student? student = await _repository.GetByIdAsync(studentId, ct);
            if (student is null) return EntityOperationResult.Failure("Student no longer exists. Refresh the list.");
            if (student.IsAllowedToBorrow == allowed)
                return EntityOperationResult.Success(allowed ? "Student is already eligible; no change was made." : "Student is already restricted; no change was made.");
            await _repository.SetEligibilityAsync(studentId, allowed, ct);
            return EntityOperationResult.Success(allowed ? "Student is now eligible for new borrowings." : "Student is restricted from new borrowings. Existing items may still be returned.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return EntityOperationResult.Failure("The eligibility change could not be saved. Check the connection and refresh the student record.");
        }
    }

}
