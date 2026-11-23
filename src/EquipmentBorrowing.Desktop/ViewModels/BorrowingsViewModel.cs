using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class BorrowingsViewModel : ViewModelBase
{
    private readonly BorrowingQueryService _queryService;
    private readonly ReturnEquipmentService _returnService;
    [ObservableProperty] private BorrowingHistoryItem? selectedBorrowing;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string statusFilter = "Active";
    [ObservableProperty] private string sortField = "Checkout date";
    [ObservableProperty] private bool descending = true;
    [ObservableProperty] private string statusMessage = "Showing active borrowing records.";
    [ObservableProperty] private bool isLoading;
    public ObservableCollection<BorrowingHistoryItem> Borrowings { get; } = [];
    public string[] StatusOptions { get; } = ["All", "Active", "Returned"];
    public string[] SortOptions { get; } = ["Checkout date", "Expected return date"];

    public BorrowingsViewModel(BorrowingQueryService queryService, ReturnEquipmentService returnService)
    {
        _queryService = queryService;
        _returnService = returnService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;

    try
    {
        var filter = Enum.TryParse<BorrowingStatusFilter>(
            StatusFilter, true, out var parsed)
            ? parsed
            : BorrowingStatusFilter.Active;

        var sort = SortField == "Expected return date"
            ? BorrowingSortField.ExpectedReturnDate
            : BorrowingSortField.CheckoutDate;

        var rows = await _queryService.SearchAsync(
            new BorrowingSearchCriteria(SearchText, filter, sort, Descending));

        Borrowings.Clear();

        foreach (var row in rows)
        {
            Borrowings.Add(row);
        }

        StatusMessage = rows.Count == 0
            ? "No borrowing records match these criteria."
            : $"Loaded {rows.Count} borrowing record(s).";
    }
    catch (Exception ex)
    {
        StatusMessage = $"Borrowing query failed: {ex.GetBaseException().Message}";
        System.Diagnostics.Debug.WriteLine(ex);
    }
    finally
    {
        IsLoading = false;
    }
}

    [RelayCommand]
    private async Task ReturnSelectedAsync()
    {
        if (SelectedBorrowing is null) { StatusMessage = "Select a borrowing record."; return; }
        if (SelectedBorrowing.Status != BorrowingStatus.Active) { StatusMessage = "Only active borrowings can be returned."; return; }
        try
        {
            var result = await _returnService.ReturnAsync(new ReturnEquipmentRequest(SelectedBorrowing.BorrowingId, DateOnly.FromDateTime(DateTime.Today)));
            await LoadAfterActionAsync(result.Message);
        }
        catch (Exception)
        {
            StatusMessage = "The return could not be confirmed. Reconnect and refresh the borrowing and equipment records before retrying.";
        }
    }

    private async Task LoadAfterActionAsync(string actionMessage)
    {
        await LoadAsync();
        if (StatusMessage.StartsWith("Unable to retrieve", StringComparison.OrdinalIgnoreCase))
            StatusMessage = actionMessage + " The list refresh also failed; reconnect and refresh before retrying.";
        else
            StatusMessage = actionMessage;
    }

    [RelayCommand]
    private async Task ClearCriteriaAsync()
    {
        SearchText = string.Empty;
        StatusFilter = "All";
        SortField = "Checkout date";
        Descending = true;
        await LoadAsync();
    }
}
