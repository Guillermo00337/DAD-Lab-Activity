namespace EquipmentBorrowing.Application.Services;

public sealed record ReturnEquipmentResult(
    bool Succeeded,
    string Message)
{
    public static ReturnEquipmentResult Success() =>
        new(true, "Equipment returned successfully.");

    public static ReturnEquipmentResult Failure(string message) =>
        new(false, message);
}
