using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class BorrowingsViewModel : ViewModelBase
{
    private readonly ActiveBorrowingsService _activeBorrowingsService;
    private readonly ReturnEquipmentService _returnEquipmentService;

    [ObservableProperty]
    private ActiveBorrowingDetails? selectedBorrowing;

    [ObservableProperty]
    private string statusMessage = "Select an active borrowing to return equipment.";

    public BorrowingsViewModel(
        ActiveBorrowingsService activeBorrowingsService,
        ReturnEquipmentService returnEquipmentService)
    {
        _activeBorrowingsService = activeBorrowingsService;
        _returnEquipmentService = returnEquipmentService;
    }

    public ObservableCollection<ActiveBorrowingDetails> ActiveBorrowings { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        ActiveBorrowings.Clear();
        foreach (ActiveBorrowingDetails borrowing in await _activeBorrowingsService.GetActiveBorrowingsAsync())
        {
            ActiveBorrowings.Add(borrowing);
        }
    }

    [RelayCommand]
    private async Task ReturnSelectedAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select an active borrowing.";
            return;
        }

        ReturnEquipmentResult result = await _returnEquipmentService.ReturnAsync(
            new ReturnEquipmentRequest(
                SelectedBorrowing.BorrowingId,
                DateOnly.FromDateTime(DateTime.Today)));

        StatusMessage = result.Message;
        SelectedBorrowing = null;
        await LoadAsync();
    }
}
