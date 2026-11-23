using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed record RegisterEquipmentRequest(string Identifier, string Name, string? Description);
public sealed record UpdateEquipmentRequest(int EquipmentId, string Identifier, string Name, string? Description);

public sealed class EquipmentManagementService
{
    private readonly IEquipmentRepository _repository;
    public EquipmentManagementService(IEquipmentRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken ct = default) => _repository.GetAllAsync(ct);

    public async Task<EntityOperationResult> RegisterAsync(RegisterEquipmentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier)) return EntityOperationResult.Failure("Equipment identifier is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) return EntityOperationResult.Failure("Equipment name is required.");
        try
        {
            if (await _repository.IdentifierExistsAsync(request.Identifier.Trim(), cancellationToken: ct))
                return EntityOperationResult.Failure("That equipment identifier is already registered (identifiers are case-insensitive).");
            await _repository.AddAsync(new Equipment(0, request.Identifier, request.Name, true, request.Description), ct);
            return EntityOperationResult.Success("Equipment registered and marked available.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return EntityOperationResult.Failure("The equipment could not be saved because of a database or uniqueness conflict. Check the connection, refresh the list and try again.");
        }
    }

    public async Task<EntityOperationResult> UpdateAsync(UpdateEquipmentRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier)) return EntityOperationResult.Failure("Equipment identifier is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) return EntityOperationResult.Failure("Equipment name is required.");
        try
        {
            Equipment? equipment = await _repository.GetByIdAsync(request.EquipmentId, ct);
            if (equipment is null) return EntityOperationResult.Failure("Equipment no longer exists. Refresh the list.");
            if (await _repository.IdentifierExistsAsync(request.Identifier.Trim(), request.EquipmentId, ct))
                return EntityOperationResult.Failure("Another equipment item already uses that identifier.");
            equipment.UpdateDetails(request.Identifier, request.Name, request.Description);
            await _repository.UpdateDetailsAsync(equipment, ct);
            return EntityOperationResult.Success("Equipment details updated. Availability and borrowing history were preserved.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return EntityOperationResult.Failure("The equipment update could not be saved because of a database or uniqueness conflict. Refresh and try again.");
        }
    }

}
