using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class HistoryViewModel : ViewModelBase
{
    private readonly StudentManagementService _students;
    private readonly EquipmentManagementService _equipment;
    private readonly BorrowingQueryService _queries;
    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string statusMessage = "Choose a student or equipment item to inspect its history.";
    [ObservableProperty] private bool isLoading;
    public ObservableCollection<Student> Students { get; } = [];
    public ObservableCollection<Equipment> EquipmentItems { get; } = [];
    public ObservableCollection<BorrowingHistoryItem> History { get; } = [];

    public HistoryViewModel(StudentManagementService students, EquipmentManagementService equipment, BorrowingQueryService queries)
    {
        _students = students; _equipment = equipment; _queries = queries;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        int? studentId = SelectedStudent?.Id;
        int? equipmentId = SelectedEquipment?.Id;
        IsLoading = true;
        try
        {
            Students.Clear(); foreach (var item in await _students.GetAllAsync()) Students.Add(item);
            EquipmentItems.Clear(); foreach (var item in await _equipment.GetAllAsync()) EquipmentItems.Add(item);
            SelectedStudent = studentId.HasValue ? Students.FirstOrDefault(x => x.Id == studentId.Value) : null;
            SelectedEquipment = equipmentId.HasValue ? EquipmentItems.FirstOrDefault(x => x.Id == equipmentId.Value) : null;
        }
        catch (Exception ex)
{
    StatusMessage = $"History error: {ex.GetBaseException().Message}";
}
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LoadStudentHistoryAsync()
    {
        if (SelectedStudent is null) { StatusMessage = "Select a student."; return; }
        int selectedId = SelectedStudent.Id;
        await LoadHistoryAsync(() => _queries.GetStudentHistoryAsync(selectedId), "Student");
    }

    [RelayCommand]
    private async Task LoadEquipmentHistoryAsync()
    {
        if (SelectedEquipment is null) { StatusMessage = "Select equipment."; return; }
        int selectedId = SelectedEquipment.Id;
        await LoadHistoryAsync(() => _queries.GetEquipmentHistoryAsync(selectedId), "Equipment");
    }

    private async Task LoadHistoryAsync(Func<Task<IReadOnlyList<BorrowingHistoryItem>?>> loader, string type)
    {
        IsLoading = true;
        try
        {
            var rows = await loader();
            History.Clear();
            if (rows is null) { StatusMessage = $"The selected {type.ToLowerInvariant()} record no longer exists. Refresh and select again."; return; }
            foreach (var row in rows) History.Add(row);
            StatusMessage = rows.Count == 0 ? $"No borrowing history exists for this {type.ToLowerInvariant()}." : $"Loaded {rows.Count} history record(s).";
        }
        catch (Exception ex)
{
    StatusMessage = $"History error: {ex.GetBaseException().Message}";
}
        finally { IsLoading = false; }
    }
}
