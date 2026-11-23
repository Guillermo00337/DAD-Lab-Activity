using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public sealed class EfStudentRepository : IStudentRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;
    public EfStudentRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory) => _factory = factory;

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Students.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Students.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(cancellationToken);
    }

    public async Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Students.Add(student);
        await db.SaveChangesAsync(cancellationToken);
        return student;
    }

    public async Task UpdateDetailsAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var current = await db.Students.SingleOrDefaultAsync(x => x.Id == student.Id, cancellationToken)
            ?? throw new InvalidOperationException("Student no longer exists.");
        // Copy only descriptive fields so a concurrent eligibility change cannot be overwritten.
        current.UpdateDetails(student.StudentNumber, student.FullName, student.ContactInformation);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetEligibilityAsync(int studentId, bool allowed, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var current = await db.Students.SingleOrDefaultAsync(x => x.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student no longer exists.");
        current.SetBorrowingEligibility(allowed);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> NumberExistsAsync(string studentNumber, int? exceptId = null, CancellationToken cancellationToken = default)
    {
        string normalized = studentNumber.Trim().ToUpperInvariant();
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Students.AnyAsync(x => x.Id != (exceptId ?? 0) && x.StudentNumber.Trim().ToUpper() == normalized, cancellationToken);
    }
}
