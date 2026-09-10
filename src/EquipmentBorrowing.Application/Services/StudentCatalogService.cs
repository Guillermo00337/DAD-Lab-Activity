using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public sealed class StudentCatalogService
{
    private readonly IStudentRepository _studentRepository;

    public StudentCatalogService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default) =>
        _studentRepository.GetAllAsync(cancellationToken);
}
