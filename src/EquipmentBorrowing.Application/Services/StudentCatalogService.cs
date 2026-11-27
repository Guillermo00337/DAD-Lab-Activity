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

    public Task<IReadOnlyList<Student>> GetStudentsAsync(
        CancellationToken cancellationToken = default) =>
        _studentRepository.GetAllAsync(cancellationToken);

    public Task<Student?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        _studentRepository.GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Student>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        string term = search?.Trim() ?? string.Empty;

        if (term.Length == 0)
            return students;

        return students
            .Where(student =>
                student.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                student.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}