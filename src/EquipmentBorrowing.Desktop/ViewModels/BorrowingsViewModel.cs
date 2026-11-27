using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class BorrowingsViewModel : ViewModelBase
{
    private readonly BorrowingApiClient _api;

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

    public BorrowingsViewModel(BorrowingApiClient api) => _api = api;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var rows = await _api.SearchBorrowingsAsync(
                SearchText, StatusFilter, SortField, Descending);

            Borrowings.Clear();
            foreach (var row in rows) Borrowings.Add(row);

            StatusMessage = rows.Count == 0
                ? "No borrowing records match these criteria."
                : $"Loaded {rows.Count} borrowing record(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Borrowing query failed: {ex.GetBaseException().Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReturnSelectedAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Select a borrowing record.";
            return;
        }

        if (SelectedBorrowing.Status != BorrowingStatus.Active)
        {
            StatusMessage = "Only active borrowings can be returned.";
            return;
        }

        try
        {
            var message = await _api.ReturnAsync(SelectedBorrowing.BorrowingId);
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"The return could not be confirmed: {ex.Message} Refresh before retrying.";
        }
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