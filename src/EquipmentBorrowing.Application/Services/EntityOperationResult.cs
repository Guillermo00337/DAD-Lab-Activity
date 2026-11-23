namespace EquipmentBorrowing.Application.Services;

public sealed record EntityOperationResult(bool Succeeded, string Message)
{
    public static EntityOperationResult Success(string message) => new(true, message);
    public static EntityOperationResult Failure(string message) => new(false, message);
}
