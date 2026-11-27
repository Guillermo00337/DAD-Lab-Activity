using EquipmentBorrowing.Api.Contracts;
using EquipmentBorrowing.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowing.Api.Controllers;

[ApiController]
[Route("api/v1/borrowings")]
public sealed class BorrowingsController : ControllerBase
{
    private readonly BorrowEquipmentService _borrowings;
    private readonly ReturnEquipmentService _returns;
    private readonly BorrowingQueryService _queries;

    public BorrowingsController(
        BorrowEquipmentService borrowings,
        ReturnEquipmentService returns,
        BorrowingQueryService queries)
    {
        _borrowings = borrowings;
        _returns = returns;
        _queries = queries;
    }

    // API-10: create one borrowing
    [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBorrowingRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StudentId is null ||
            request.EquipmentId is null ||
            request.ExpectedReturnDate is null)
        {
            return BadRequest(new ApiError("validation_error", "Student, equipment, and expected return date are required."));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (request.ExpectedReturnDate <= today)
        {
            return BadRequest(new ApiError("validation_error", "Expected return date must be later than today."));
        }

        var result = await _borrowings.BorrowAsync(
            new BorrowEquipmentRequest(
                request.StudentId.Value,
                request.EquipmentId.Value,
                today,
                request.ExpectedReturnDate.Value),
            cancellationToken);

        if (!result.Succeeded || result.Borrowing is null)
            return Failure(result.Message);

        var item = await FindHistoryItem(result.Borrowing.Id, cancellationToken);
        if (item is null)
            return StatusCode(500, new ApiError("readback_failed", "The borrowing was created but could not be read back."));

        return CreatedAtRoute("GetBorrowingById", new { id = item.BorrowingId }, ToResponse(item));
    }

    // API-11: create multiple borrowings as one checkout
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpPost("/api/v1/checkout-requests")]
    public async Task<IActionResult> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (request.StudentId is null ||
            request.EquipmentIds is null ||
            request.ExpectedReturnDate is null)
        {
            return BadRequest(new ApiError("validation_error", "Student, equipment IDs, and expected return date are required."));
        }

        if (request.EquipmentIds.Count < 2 ||
            request.EquipmentIds.Distinct().Count() != request.EquipmentIds.Count)
        {
            return BadRequest(new ApiError("validation_error", "Select at least two distinct equipment items."));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (request.ExpectedReturnDate <= today)
        {
            return BadRequest(new ApiError("validation_error", "Expected return date must be later than today."));
        }

        var result = await _borrowings.BorrowMultipleAsync(
            request.StudentId.Value,
            request.EquipmentIds,
            today,
            request.ExpectedReturnDate.Value,
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var items = new List<BorrowingResponse>();
        foreach (var borrowing in result.Borrowings)
        {
            var item = await FindHistoryItem(borrowing.Id, cancellationToken);
            if (item is null)
                return StatusCode(500, new ApiError("readback_failed", "The checkout succeeded but could not be read back."));
            items.Add(ToResponse(item));
        }

        return StatusCode(StatusCodes.Status201Created, new CheckoutResponse(items));
    }

    // API-12: return one borrowing
    [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(
        int id,
        [FromBody] EmptyJsonObject request,
        CancellationToken cancellationToken)
    {
        var existing = await FindHistoryItem(id, cancellationToken);
        if (existing is null)
            return NotFound(new ApiError("not_found", "Borrowing was not found."));

        var result = await _returns.ReturnAsync(
            new ReturnEquipmentRequest(id, DateOnly.FromDateTime(DateTime.Today)),
            cancellationToken);

        if (!result.Succeeded)
            return Failure(result.Message);

        var updated = await FindHistoryItem(id, cancellationToken);
        return updated is null
            ? StatusCode(500, new ApiError("readback_failed", "The return succeeded but could not be read back."))
            : Ok(ToResponse(updated));
    }

    // API-13: get one borrowing
    [ProducesResponseType(typeof(BorrowingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpGet("{id:int}", Name = "GetBorrowingById")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await FindHistoryItem(id, cancellationToken);
        return item is null
            ? NotFound(new ApiError("not_found", "Borrowing was not found."))
            : Ok(ToResponse(item));
    }

    // API-14: search and sort borrowing records
    [ProducesResponseType(typeof(IReadOnlyList<BorrowingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string status = "all",
        [FromQuery] string sort = "borrowedDate",
        [FromQuery] string direction = "asc",
        CancellationToken cancellationToken = default)
    {
        var statusFilter = status.Trim().ToLowerInvariant() switch
        {
            "all" => BorrowingStatusFilter.All,
            "active" => BorrowingStatusFilter.Active,
            "returned" => BorrowingStatusFilter.Returned,
            _ => (BorrowingStatusFilter?)null
        };

        if (statusFilter is null)
            return BadRequest(new ApiError("validation_error", "Status must be all, active, or returned."));

        var sortField = sort.Trim().ToLowerInvariant() switch
        {
            "borroweddate" => BorrowingSortField.CheckoutDate,
            "expectedreturndate" => BorrowingSortField.ExpectedReturnDate,
            _ => (BorrowingSortField?)null
        };

        if (sortField is null)
            return BadRequest(new ApiError("validation_error", "Sort must be borrowedDate or expectedReturnDate."));

        var descending = direction.Trim().ToLowerInvariant() switch
        {
            "asc" => false,
            "desc" => true,
            _ => (bool?)null
        };

        if (descending is null)
            return BadRequest(new ApiError("validation_error", "Direction must be asc or desc."));

        var criteria = new BorrowingSearchCriteria(
            SearchText: string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            Status: statusFilter.Value,
            SortBy: sortField.Value,
            Descending: descending.Value);

        var items = await _queries.SearchAsync(criteria, cancellationToken);
        return Ok(items.Select(ToResponse).ToList());
    }

    // API-15: get a student's borrowing history
    [ProducesResponseType(typeof(IReadOnlyList<BorrowingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v1/students/{id:int}/borrowings")]
    public async Task<IActionResult> GetStudentHistory(
        int id,
        CancellationToken cancellationToken)
    {
        var items = await _queries.GetStudentHistoryAsync(id, cancellationToken);

        return items is null
            ? NotFound(new ApiError("not_found", "Student was not found."))
            : Ok(items.Select(ToResponse).ToList());
    }

    // API-16: get an equipment item's borrowing history
    [ProducesResponseType(typeof(IReadOnlyList<BorrowingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v1/equipment/{id:int}/borrowings")]
    public async Task<IActionResult> GetEquipmentHistory(
        int id,
        CancellationToken cancellationToken)
    {
        var items = await _queries.GetEquipmentHistoryAsync(id, cancellationToken);

        return items is null
            ? NotFound(new ApiError("not_found", "Equipment was not found."))
            : Ok(items.Select(ToResponse).ToList());
    }

    private async Task<BorrowingHistoryItem?> FindHistoryItem(int id, CancellationToken cancellationToken)
    {
        var items = await _queries.SearchAsync(new BorrowingSearchCriteria(), cancellationToken);
        return items.FirstOrDefault(item => item.BorrowingId == id);
    }

    private static BorrowingResponse ToResponse(BorrowingHistoryItem item) =>
        new(
            item.BorrowingId,
            item.StudentId,
            item.EquipmentId,
            item.DateBorrowed,
            item.ExpectedReturnDate,
            item.DateReturned,
            item.Status.ToString(),
            item.StudentNumber,
            item.StudentName,
            item.EquipmentIdentifier,
            item.EquipmentName);

    private IActionResult Failure(string message)
    {
        var lower = message.ToLowerInvariant();

        if (lower.Contains("not found") || lower.Contains("no longer exists"))
            return NotFound(new ApiError("not_found", message));

        if (lower.Contains("date") ||
            lower.Contains("required") ||
            lower.Contains("duplicate") ||
            lower.Contains("at least"))
            return BadRequest(new ApiError("validation_error", message));

        return Conflict(new ApiError("conflict", message));
    }
}