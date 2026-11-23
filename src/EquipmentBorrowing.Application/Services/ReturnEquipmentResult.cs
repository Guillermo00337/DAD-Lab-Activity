namespace EquipmentBorrowing.Application.Services;

public sealed record ReturnEquipmentResult(bool Succeeded, string Message)
{
    public static ReturnEquipmentResult Success(string message = "Equipment returned successfully.") => new(true, message);
    public static ReturnEquipmentResult Failure(string message) => new(false, message);
}
