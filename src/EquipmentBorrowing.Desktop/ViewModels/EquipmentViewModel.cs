using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentViewModel : ViewModelBase
{
    private readonly EquipmentCatalogService _equipmentCatalog;
    private readonly StudentCatalogService _studentCatalog;
    private readonly BorrowEquipmentService _borrowService;

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private DateTimeOffset? expectedReturnDate = DateTimeOffset.Now.AddDays(7);
    [ObservableProperty] private string statusMessage = "Choose a student and available item.";
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Student> Students { get; } = [];
    public ObservableCollection<Equipment> EquipmentItems { get; } = [];
    public ObservableCollection<Equipment> AvailableEquipment { get; } = [];
    public ObservableCollection<EquipmentChoice> MultiItemChoices { get; } = [];

    public EquipmentViewModel(EquipmentCatalogService equipmentCatalog, StudentCatalogService studentCatalog, BorrowEquipmentService borrowService)
    {
        _equipmentCatalog = equipmentCatalog;
        _studentCatalog = studentCatalog;
        _borrowService = borrowService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        int? selectedStudentId = SelectedStudent?.Id;
        int? selectedEquipmentId = SelectedEquipment?.Id;
        try
        {
            Students.Clear();
            foreach (Student item in await _studentCatalog.GetStudentsAsync()) Students.Add(item);
            EquipmentItems.Clear();
            AvailableEquipment.Clear();
            MultiItemChoices.Clear();
            IReadOnlyList<Equipment> allEquipment = await _equipmentCatalog.GetEquipmentAsync();
            IReadOnlyList<Equipment> availableEquipment = await _equipmentCatalog.GetAvailableEquipmentAsync();
            foreach (Equipment item in allEquipment)
            {
                EquipmentItems.Add(item);
                MultiItemChoices.Add(new EquipmentChoice(item));
            }
            foreach (Equipment item in availableEquipment) AvailableEquipment.Add(item);
            SelectedStudent = selectedStudentId.HasValue ? Students.FirstOrDefault(x => x.Id == selectedStudentId.Value) : null;
            SelectedEquipment = selectedEquipmentId.HasValue ? AvailableEquipment.FirstOrDefault(x => x.Id == selectedEquipmentId.Value) : null;
            StatusMessage = $"Loaded {EquipmentItems.Count} equipment records; {AvailableEquipment.Count} are available.";
        }
        catch (Exception)
        {
            StatusMessage = "Unable to load the catalog. Check PostgreSQL and choose Refresh to retry.";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (!TryGetDates(out DateOnly checkout, out DateOnly due)) return;
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
            var result = await _borrowService.BorrowAsync(new BorrowEquipmentRequest(SelectedStudent.Id, SelectedEquipment.Id, checkout, due));
            await RefreshAfterOperationAsync(result.Message);
        }
        catch (Exception) { StatusMessage = "Borrowing failed. Refresh and verify the database before retrying."; }
    }

    [RelayCommand]
    private async Task BorrowMultipleAsync()
    {
        if (!TryGetDates(out DateOnly checkout, out DateOnly due)) return;
        if (SelectedStudent is null) { StatusMessage = "Select a student."; return; }
        int[] ids = MultiItemChoices.Where(x => x.IsSelected).Select(x => x.Id).ToArray();
        try
        {
            CheckoutResult result = await _borrowService.BorrowMultipleAsync(SelectedStudent.Id, ids, checkout, due);
            await RefreshAfterOperationAsync(result.Message);
        }
        catch (Exception) { StatusMessage = "Multi-item checkout failed. Reconnect and refresh before deciding whether to retry."; }
    }

    private async Task RefreshAfterOperationAsync(string operationMessage)
    {
        await LoadAsync();
        if (StatusMessage.StartsWith("Unable to load", StringComparison.OrdinalIgnoreCase))
            StatusMessage = operationMessage + " The catalog refresh also failed; use Refresh after reconnecting.";
        else
            StatusMessage = operationMessage;
    }

    private bool TryGetDates(out DateOnly checkout, out DateOnly due)
    {
        checkout = DateOnly.FromDateTime(DateTime.Today);
        due = default;
        if (ExpectedReturnDate is null)
        {
            StatusMessage = "Choose an expected return date.";
            return false;
        }
        due = DateOnly.FromDateTime(ExpectedReturnDate.Value.DateTime);
        if (due <= checkout)
        {
            StatusMessage = "Expected return date must be later than today.";
            return false;
        }
        return true;
    }
}
