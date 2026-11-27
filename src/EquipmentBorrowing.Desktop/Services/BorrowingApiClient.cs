using EquipmentBorrowing.Application.Services;
using System.Net.Http.Json;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.Services;

public sealed class BorrowingApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<Student>> GetStudentsAsync()
    {
        var rows = await http.GetFromJsonAsync<List<StudentResponse>>("api/v1/students")
            ?? [];

        return rows.Select(x => new Student(
            x.Id, x.StudentNumber, x.FullName,
            x.IsEligible, contactInformation: x.Contact)).ToList();
    }

    public async Task<IReadOnlyList<Equipment>> GetEquipmentAsync(bool availableOnly = false)
    {
        var path = availableOnly
            ? "api/v1/equipment?available=true"
            : "api/v1/equipment";

        var rows = await http.GetFromJsonAsync<List<EquipmentResponse>>(path)
            ?? [];

        return rows.Select(x => new Equipment(
            x.Id, x.EquipmentCode, x.Name, x.IsAvailable, x.Description)).ToList();
    }

    public async Task<string> BorrowAsync(int studentId, int equipmentId, DateOnly expectedReturnDate)
    {
        using var response = await http.PostAsJsonAsync("api/v1/borrowings",
            new { studentId, equipmentId, expectedReturnDate });

        await EnsureSuccessAsync(response);
        return "Borrowing approved.";
    }

    public async Task<string> BorrowMultipleAsync(
        int studentId, IReadOnlyCollection<int> equipmentIds, DateOnly expectedReturnDate)
    {
        using var response = await http.PostAsJsonAsync("api/v1/checkout-requests",
            new { studentId, equipmentIds, expectedReturnDate });

        await EnsureSuccessAsync(response);
        return $"{equipmentIds.Count} items checked out successfully.";
    }

    public async Task<IReadOnlyList<BorrowingHistoryItem>> SearchBorrowingsAsync(
        string? search, string status, string sort, bool descending)
    {
        var statusValue = Uri.EscapeDataString(status.ToLowerInvariant());
        var sortValue = sort == "Expected return date" ? "expectedReturnDate" : "borrowedDate";
        var direction = descending ? "desc" : "asc";
        var searchPart = string.IsNullOrWhiteSpace(search)
            ? ""
            : $"&search={Uri.EscapeDataString(search.Trim())}";

        var rows = await http.GetFromJsonAsync<List<BorrowingResponse>>(
            $"api/v1/borrowings?status={statusValue}&sort={sortValue}&direction={direction}{searchPart}")
            ?? [];

                return rows.Select(ToHistoryItem).ToList();
    }

    public async Task<string> RegisterStudentAsync(
        string studentNumber, string fullName, string? contact)
    {
        using var response = await http.PostAsJsonAsync("api/v1/students",
            new { studentNumber, fullName, contact });
        await EnsureSuccessAsync(response);
        return "Student registered successfully.";
    }

    public async Task<string> UpdateStudentAsync(
        int id, string studentNumber, string fullName, string? contact)
    {
        using var response = await http.PutAsJsonAsync($"api/v1/students/{id}",
            new { studentNumber, fullName, contact });
        await EnsureSuccessAsync(response);
        return "Student details updated successfully.";
    }

    public async Task<string> SetStudentEligibilityAsync(int id, bool isEligible)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/v1/students/{id}/eligibility", new { isEligible });
        await EnsureSuccessAsync(response);
        return "Student eligibility updated successfully.";
    }

    public async Task<string> RegisterEquipmentAsync(
        string equipmentCode, string name, string? description)
    {
        using var response = await http.PostAsJsonAsync("api/v1/equipment",
            new { equipmentCode, name, description });
        await EnsureSuccessAsync(response);
        return "Equipment registered successfully.";
    }

    public async Task<string> UpdateEquipmentAsync(
        int id, string equipmentCode, string name, string? description)
    {
        using var response = await http.PutAsJsonAsync($"api/v1/equipment/{id}",
            new { equipmentCode, name, description });
        await EnsureSuccessAsync(response);
        return "Equipment details updated successfully.";
    }

    public Task<IReadOnlyList<BorrowingHistoryItem>> GetStudentHistoryAsync(int studentId) =>
        GetHistoryAsync($"api/v1/students/{studentId}/borrowings");

    public Task<IReadOnlyList<BorrowingHistoryItem>> GetEquipmentHistoryAsync(int equipmentId) =>
        GetHistoryAsync($"api/v1/equipment/{equipmentId}/borrowings");

    private async Task<IReadOnlyList<BorrowingHistoryItem>> GetHistoryAsync(string path)
    {
        var rows = await http.GetFromJsonAsync<List<BorrowingResponse>>(path) ?? [];
        return rows.Select(ToHistoryItem).ToList();
    }

    private static BorrowingHistoryItem ToHistoryItem(BorrowingResponse x) =>
        new(
            x.Id, x.StudentId, x.StudentNumber, x.StudentName,
            x.EquipmentId, x.EquipmentCode, x.EquipmentName,
            x.BorrowedDate, x.ExpectedReturnDate, x.ReturnedDate,
            Enum.Parse<BorrowingStatus>(x.Status, ignoreCase: true));

    public async Task<string> ReturnAsync(int borrowingId)
    {
        using var response = await http.PostAsJsonAsync(
            $"api/v1/borrowings/{borrowingId}/return", new { });

        await EnsureSuccessAsync(response);
        return "Equipment returned successfully.";
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        var message = error?.Message ?? $"API request failed ({(int)response.StatusCode}).";
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private sealed record BorrowingResponse(
        int Id, int StudentId, int EquipmentId,
        DateOnly BorrowedDate, DateOnly ExpectedReturnDate, DateOnly? ReturnedDate,
        string Status, string StudentNumber, string StudentName,
        string EquipmentCode, string EquipmentName);

    private sealed record StudentResponse(
        int Id, string StudentNumber, string FullName, string? Contact, bool IsEligible);

    private sealed record EquipmentResponse(
        int Id, string EquipmentCode, string Name, string? Description, bool IsAvailable);

    private sealed record ApiError(string Code, string Message);
}