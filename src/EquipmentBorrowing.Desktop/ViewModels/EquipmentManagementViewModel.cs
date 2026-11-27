using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentManagementViewModel : ViewModelBase
{
    private readonly BorrowingApiClient _api;

    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string identifier = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string statusMessage = "Register or select equipment to edit its descriptive information.";
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Equipment> EquipmentItems { get; } = [];

    public EquipmentManagementViewModel(BorrowingApiClient api) => _api = api;

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (value is null) return;
        Identifier = value.AssetTag;
        Name = value.Name;
        Description = value.Description ?? string.Empty;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        int? selectedId = SelectedEquipment?.Id;
        IsLoading = true;
        try
        {
            EquipmentItems.Clear();
            foreach (var item in await _api.GetEquipmentAsync()) EquipmentItems.Add(item);
            SelectedEquipment = selectedId.HasValue
                ? EquipmentItems.FirstOrDefault(x => x.Id == selectedId.Value)
                : null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to load equipment from the API: {ex.GetBaseException().Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        try
        {
            var message = await _api.RegisterEquipmentAsync(Identifier, Name, Description);
            ClearForm();
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Equipment registration failed: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (SelectedEquipment is null)
        {
            StatusMessage = "Select equipment first.";
            return;
        }

        try
        {
            var message = await _api.UpdateEquipmentAsync(
                SelectedEquipment.Id, Identifier, Name, Description);
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Equipment update failed: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedEquipment = null;
        Identifier = Name = Description = string.Empty;
    }
}