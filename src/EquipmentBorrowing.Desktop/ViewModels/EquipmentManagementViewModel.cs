using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentManagementViewModel : ViewModelBase
{
    private readonly EquipmentManagementService _service;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string identifier = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string statusMessage = "Register or select equipment to edit its descriptive information.";
    [ObservableProperty] private bool isLoading;
    public ObservableCollection<Equipment> EquipmentItems { get; } = [];

    public EquipmentManagementViewModel(EquipmentManagementService service) => _service = service;

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
            foreach (var item in await _service.GetAllAsync()) EquipmentItems.Add(item);
            SelectedEquipment = selectedId.HasValue ? EquipmentItems.FirstOrDefault(x => x.Id == selectedId.Value) : null;
        }
        catch (Exception) { StatusMessage = "Unable to load equipment. Check PostgreSQL and retry."; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        var result = await _service.RegisterAsync(new RegisterEquipmentRequest(Identifier, Name, Description));
        if (result.Succeeded)
        {
            ClearForm();
            await LoadAsync();
            if (StatusMessage.StartsWith("Unable to load", StringComparison.OrdinalIgnoreCase))
                StatusMessage = result.Message + " The list refresh also failed; reconnect and refresh.";
            else StatusMessage = result.Message;
        }
        else StatusMessage = result.Message;
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (SelectedEquipment is null) { StatusMessage = "Select equipment first."; return; }
        var result = await _service.UpdateAsync(new UpdateEquipmentRequest(SelectedEquipment.Id, Identifier, Name, Description));
        if (result.Succeeded)
        {
            await LoadAsync();
            if (StatusMessage.StartsWith("Unable to load", StringComparison.OrdinalIgnoreCase))
                StatusMessage = result.Message + " The list refresh also failed; reconnect and refresh.";
            else StatusMessage = result.Message;
        }
        else StatusMessage = result.Message;
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedEquipment = null;
        Identifier = Name = Description = string.Empty;
    }
}
