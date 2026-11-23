using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default);
    Task UpdateDetailsAsync(Student student, CancellationToken cancellationToken = default);
    Task SetEligibilityAsync(int studentId, bool allowed, CancellationToken cancellationToken = default);
    Task<bool> NumberExistsAsync(string studentNumber, int? exceptId = null, CancellationToken cancellationToken = default);
}
