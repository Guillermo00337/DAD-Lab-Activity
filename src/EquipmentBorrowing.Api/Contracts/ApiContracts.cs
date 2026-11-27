using System.ComponentModel.DataAnnotations;

namespace EquipmentBorrowing.Api.Contracts;

public sealed record StudentResponse(
    int Id,
    string StudentNumber,
    string FullName,
    string? Contact,
    bool IsEligible);

public sealed record EquipmentResponse(
    int Id,
    string EquipmentCode,
    string Name,
    string? Description,
    bool IsAvailable);

public sealed record BorrowingResponse(
    int Id,
    int StudentId,
    int EquipmentId,
    DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate,
    DateOnly? ReturnedDate,
    string Status,
    string StudentNumber,
    string StudentName,
    string EquipmentCode,
    string EquipmentName);

public sealed record ApiError(string Code, string Message);

public sealed class CreateStudentRequest
{
    [Required] public string? StudentNumber { get; init; }
    [Required] public string? FullName { get; init; }
    public string? Contact { get; init; }
}

public sealed class UpdateStudentRequest
{
    [Required] public string? StudentNumber { get; init; }
    [Required] public string? FullName { get; init; }
    public string? Contact { get; init; }
}

public sealed class SetEligibilityRequest
{
    [Required] public bool? IsEligible { get; init; }
}

public sealed class CreateEquipmentRequest
{
    [Required] public string? EquipmentCode { get; init; }
    [Required] public string? Name { get; init; }
    public string? Description { get; init; }
}

public sealed class UpdateEquipmentRequest
{
    [Required] public string? EquipmentCode { get; init; }
    [Required] public string? Name { get; init; }
    public string? Description { get; init; }
}

public sealed class CreateBorrowingRequest
{
    [Required] public int? StudentId { get; init; }
    [Required] public int? EquipmentId { get; init; }
    [Required] public DateOnly? ExpectedReturnDate { get; init; }
}

public sealed class CreateCheckoutRequest
{
    [Required] public int? StudentId { get; init; }
    [Required] public List<int>? EquipmentIds { get; init; }
    [Required] public DateOnly? ExpectedReturnDate { get; init; }
}

public sealed record CheckoutResponse(
    IReadOnlyList<BorrowingResponse> Borrowings);

public sealed class EmptyJsonObject
{
}