using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class CatalogController(
    EquipmentCatalogService equipmentCatalog,
    StudentCatalogService studentCatalog) : ControllerBase
{
    [HttpGet("equipment")]
    [ProducesResponseType(typeof(IReadOnlyList<EquipmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetEquipment(
        [FromQuery] bool? available,
        CancellationToken cancellationToken)
    {
        var equipment = await equipmentCatalog.GetEquipmentAsync(cancellationToken);

        if (available.HasValue)
            equipment = equipment.Where(item => item.IsAvailable == available.Value).ToList();

        return Ok(equipment.Select(ToResponse).ToArray());
    }

    [HttpGet("students")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StudentResponse>>> GetStudents(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var students = await studentCatalog.SearchAsync(search, cancellationToken);
        return Ok(students.Select(ToResponse).ToArray());
    }

    [HttpGet("students/{id:int}", Name = "GetStudentById")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponse>> GetStudent(
        int id,
        CancellationToken cancellationToken)
    {
        var student = await studentCatalog.GetByIdAsync(id, cancellationToken);

        return student is null
            ? NotFound(new ApiError("not_found", "Student was not found."))
            : Ok(ToResponse(student));
    }

    [HttpGet("equipment/{id:int}", Name = "GetEquipmentById")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetEquipmentById(
        int id,
        CancellationToken cancellationToken)
    {
        var equipment = await equipmentCatalog.GetByIdAsync(id, cancellationToken);

        return equipment is null
            ? NotFound(new ApiError("not_found", "Equipment was not found."))
            : Ok(ToResponse(equipment));
    }

    private static StudentResponse ToResponse(Student student) =>
        new(
            student.Id,
            student.StudentNumber,
            student.FullName,
            student.ContactInformation,
            student.IsAllowedToBorrow);

    private static EquipmentResponse ToResponse(Equipment equipment) =>
        new(
            equipment.Id,
            equipment.AssetTag,
            equipment.Name,
            equipment.Description,
            equipment.IsAvailable);
}