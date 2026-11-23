using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Equipment>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<Equipment> AddAsync(Equipment equipment, CancellationToken cancellationToken = default);
    Task UpdateDetailsAsync(Equipment equipment, CancellationToken cancellationToken = default);
    Task<bool> IdentifierExistsAsync(string identifier, int? exceptId = null, CancellationToken cancellationToken = default);
}
