using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
public sealed class StudentsController(
    StudentManagementService studentManagement,
    StudentCatalogService studentCatalog) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentManagement.RegisterAsync(
            new RegisterStudentRequest(
                request.StudentNumber!,
                request.FullName!,
                request.Contact),
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var matches = await studentCatalog.SearchAsync(
            request.StudentNumber,
            cancellationToken);

        var student = matches.FirstOrDefault(x =>
            string.Equals(
                x.StudentNumber.Trim(),
                request.StudentNumber!.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (student is null)
            return StatusCode(500, new ApiError(
                "server_error",
                "Student was saved, but the created record could not be read back."));

        var response = ToResponse(student);

        return CreatedAtRoute(
            "GetStudentById",
            new { id = student.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        EquipmentBorrowing.Api.Contracts.UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await studentCatalog.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new ApiError("not_found", "Student was not found."));

        var result = await studentManagement.UpdateAsync(
            new EquipmentBorrowing.Application.Services.UpdateStudentRequest(
                id,
                request.StudentNumber!,
                request.FullName!,
                request.Contact),
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var updated = await studentCatalog.GetByIdAsync(id, cancellationToken);
        return updated is null
            ? StatusCode(500, new ApiError(
                "server_error",
                "Updated student could not be read back."))
            : Ok(ToResponse(updated));
    }

    [HttpPatch("{id:int}/eligibility")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetEligibility(
        int id,
        SetEligibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await studentManagement.SetEligibilityAsync(
            id,
            request.IsEligible!.Value,
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var student = await studentCatalog.GetByIdAsync(id, cancellationToken);
        return student is null
            ? NotFound(new ApiError("not_found", "Student was not found."))
            : Ok(ToResponse(student));
    }

    private IActionResult Failure(string message)
    {
        if (message.Contains("required", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ApiError("validation_error", message));

        if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already uses", StringComparison.OrdinalIgnoreCase))
            return Conflict(new ApiError("duplicate", message));

        if (message.Contains("no longer exists", StringComparison.OrdinalIgnoreCase))
            return NotFound(new ApiError("not_found", message));

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new ApiError("server_error", message));
    }

    private static StudentResponse ToResponse(Student student) =>
        new(
            student.Id,
            student.StudentNumber,
            student.FullName,
            student.ContactInformation,
            student.IsAllowedToBorrow);
}