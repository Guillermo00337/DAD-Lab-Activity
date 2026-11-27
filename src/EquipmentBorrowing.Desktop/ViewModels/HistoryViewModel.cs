using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class HistoryViewModel : ViewModelBase
{
    private readonly BorrowingApiClient _api;

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string statusMessage = "Choose a student or equipment item to inspect its history.";
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Student> Students { get; } = [];
    public ObservableCollection<Equipment> EquipmentItems { get; } = [];
    public ObservableCollection<BorrowingHistoryItem> History { get; } = [];

    public HistoryViewModel(BorrowingApiClient api) => _api = api;

    [RelayCommand]
    public async Task LoadAsync()
    {
        int? studentId = SelectedStudent?.Id;
        int? equipmentId = SelectedEquipment?.Id;
        IsLoading = true;
        try
        {
            Students.Clear();
            foreach (var item in await _api.GetStudentsAsync()) Students.Add(item);

            EquipmentItems.Clear();
            foreach (var item in await _api.GetEquipmentAsync()) EquipmentItems.Add(item);

            SelectedStudent = studentId.HasValue
                ? Students.FirstOrDefault(x => x.Id == studentId.Value)
                : null;
            SelectedEquipment = equipmentId.HasValue
                ? EquipmentItems.FirstOrDefault(x => x.Id == equipmentId.Value)
                : null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"History error: {ex.GetBaseException().Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadStudentHistoryAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student.";
            return;
        }

        await LoadHistoryAsync(
            () => _api.GetStudentHistoryAsync(SelectedStudent.Id), "student");
    }

    [RelayCommand]
    private async Task LoadEquipmentHistoryAsync()
    {
        if (SelectedEquipment is null)
        {
            StatusMessage = "Select equipment.";
            return;
        }

        await LoadHistoryAsync(
            () => _api.GetEquipmentHistoryAsync(SelectedEquipment.Id), "equipment");
    }

    private async Task LoadHistoryAsync(
        Func<Task<IReadOnlyList<BorrowingHistoryItem>>> loader, string type)
    {
        IsLoading = true;
        try
        {
            var rows = await loader();
            History.Clear();
            foreach (var row in rows) History.Add(row);

            StatusMessage = rows.Count == 0
                ? $"No borrowing history exists for this {type}."
                : $"Loaded {rows.Count} history record(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"History error: {ex.GetBaseException().Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}