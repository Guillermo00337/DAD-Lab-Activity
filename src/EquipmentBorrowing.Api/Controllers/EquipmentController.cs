using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1/equipment")]
public sealed class EquipmentController(
    EquipmentManagementService equipmentManagement,
    EquipmentCatalogService equipmentCatalog) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        CreateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await equipmentManagement.RegisterAsync(
            new RegisterEquipmentRequest(
                request.EquipmentCode!,
                request.Name!,
                request.Description),
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var equipment = (await equipmentCatalog.GetEquipmentAsync(cancellationToken))
            .FirstOrDefault(item => string.Equals(
                item.AssetTag.Trim(),
                request.EquipmentCode!.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (equipment is null)
            return StatusCode(500, new ApiError(
                "server_error",
                "Equipment was saved, but the created record could not be read back."));

        var response = ToResponse(equipment);

        return CreatedAtRoute(
            "GetEquipmentById",
            new { id = equipment.Id },
            response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        int id,
        EquipmentBorrowing.Api.Contracts.UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await equipmentCatalog.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return NotFound(new ApiError("not_found", "Equipment was not found."));

        var result = await equipmentManagement.UpdateAsync(
            new EquipmentBorrowing.Application.Services.UpdateEquipmentRequest(
                id,
                request.EquipmentCode!,
                request.Name!,
                request.Description),
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var updated = await equipmentCatalog.GetByIdAsync(id, cancellationToken);
        return updated is null
            ? StatusCode(500, new ApiError(
                "server_error",
                "Updated equipment could not be read back."))
            : Ok(ToResponse(updated));
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

    private static EquipmentResponse ToResponse(Equipment equipment) =>
        new(
            equipment.Id,
            equipment.AssetTag,
            equipment.Name,
            equipment.Description,
            equipment.IsAvailable);
}