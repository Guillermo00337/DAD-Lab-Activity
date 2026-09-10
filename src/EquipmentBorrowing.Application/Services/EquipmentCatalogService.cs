using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class EquipmentCatalogService
{
    private readonly IEquipmentRepository _equipmentRepository;

    public EquipmentCatalogService(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    public Task<IReadOnlyList<Equipment>> GetEquipmentAsync(CancellationToken cancellationToken = default) =>
        _equipmentRepository.GetAllAsync(cancellationToken);
}
