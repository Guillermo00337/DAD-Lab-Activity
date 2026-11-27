using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentViewModel : ViewModelBase
{
    private readonly BorrowingApiClient _api;

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private DateTimeOffset? expectedReturnDate = DateTimeOffset.Now.AddDays(7);
    [ObservableProperty] private string statusMessage = "Choose a student and available item.";
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Student> Students { get; } = [];
    public ObservableCollection<Equipment> EquipmentItems { get; } = [];
    public ObservableCollection<Equipment> AvailableEquipment { get; } = [];
    public ObservableCollection<EquipmentChoice> MultiItemChoices { get; } = [];

    public EquipmentViewModel(BorrowingApiClient api) => _api = api;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        int? selectedStudentId = SelectedStudent?.Id;
        int? selectedEquipmentId = SelectedEquipment?.Id;
        try
        {
            Students.Clear();
            foreach (var item in await _api.GetStudentsAsync()) Students.Add(item);

            EquipmentItems.Clear();
            AvailableEquipment.Clear();
            MultiItemChoices.Clear();

            var allEquipment = await _api.GetEquipmentAsync();
            var availableEquipment = await _api.GetEquipmentAsync(availableOnly: true);

            foreach (var item in allEquipment)
            {
                EquipmentItems.Add(item);
                MultiItemChoices.Add(new EquipmentChoice(item));
            }
            foreach (var item in availableEquipment) AvailableEquipment.Add(item);

            SelectedStudent = selectedStudentId.HasValue
                ? Students.FirstOrDefault(x => x.Id == selectedStudentId.Value)
                : null;
            SelectedEquipment = selectedEquipmentId.HasValue
                ? AvailableEquipment.FirstOrDefault(x => x.Id == selectedEquipmentId.Value)
                : null;

            StatusMessage = $"Loaded {EquipmentItems.Count} equipment records; {AvailableEquipment.Count} are available.";
        }
        catch (Exception)
        {
            StatusMessage = "Unable to load the catalog from the API. Check the API connection and choose Refresh to retry.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (!TryGetDueDate(out var due)) return;

        if (SelectedStudent is null || SelectedEquipment is null)
        {
            StatusMessage = "Select a student and one equipment item.";
            return;
        }

        if (!SelectedEquipment.IsAvailable)
        {
            StatusMessage = "The selected equipment is not available.";
            return;
        }

        try
        {
            var message = await _api.BorrowAsync(SelectedStudent.Id, SelectedEquipment.Id, due);
            await RefreshAfterOperationAsync(message);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Borrowing failed: {ex.Message} Refresh and verify the borrowing before retrying.";
        }
    }

    [RelayCommand]
    private async Task BorrowMultipleAsync()
    {
        if (!TryGetDueDate(out var due)) return;

        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student.";
            return;
        }

        var ids = MultiItemChoices.Where(x => x.IsSelected).Select(x => x.Id).ToArray();

        try
        {
            var message = await _api.BorrowMultipleAsync(SelectedStudent.Id, ids, due);
            await RefreshAfterOperationAsync(message);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Multi-item checkout failed: {ex.Message} Reconnect and refresh before deciding whether to retry.";
        }
    }

    private async Task RefreshAfterOperationAsync(string operationMessage)
    {
        await LoadAsync();
        if (StatusMessage.StartsWith("Unable to load", StringComparison.OrdinalIgnoreCase))
            StatusMessage = operationMessage + " The catalog refresh also failed; use Refresh after reconnecting.";
        else
            StatusMessage = operationMessage;
    }

    private bool TryGetDueDate(out DateOnly due)
    {
        due = default;
        if (ExpectedReturnDate is null)
        {
            StatusMessage = "Choose an expected return date.";
            return false;
        }

        due = DateOnly.FromDateTime(ExpectedReturnDate.Value.DateTime);
        if (due <= DateOnly.FromDateTime(DateTime.Today))
        {
            StatusMessage = "Expected return date must be later than today.";
            return false;
        }

        return true;
    }
}