using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentViewModel : ViewModelBase
{
    private readonly EquipmentCatalogService _equipmentCatalogService;
    private readonly StudentCatalogService _studentCatalogService;
    private readonly BorrowEquipmentService _borrowEquipmentService;

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private DateTimeOffset? expectedReturnDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string statusMessage = "Select a student and available equipment to begin.";

    public EquipmentViewModel(
        EquipmentCatalogService equipmentCatalogService,
        StudentCatalogService studentCatalogService,
        BorrowEquipmentService borrowEquipmentService)
    {
        _equipmentCatalogService = equipmentCatalogService;
        _studentCatalogService = studentCatalogService;
        _borrowEquipmentService = borrowEquipmentService;
    }

    public ObservableCollection<Student> Students { get; } = [];

    public ObservableCollection<Equipment> EquipmentItems { get; } = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        Students.Clear();
        foreach (Student student in await _studentCatalogService.GetStudentsAsync())
        {
            Students.Add(student);
        }

        EquipmentItems.Clear();
        foreach (Equipment equipment in await _equipmentCatalogService.GetEquipmentAsync())
        {
            EquipmentItems.Add(equipment);
        }
    }

    [RelayCommand]
    private async Task BorrowAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select equipment.";
            return;
        }

        if (ExpectedReturnDate is null)
        {
            StatusMessage = "Please select an expected return date.";
            return;
        }

        DateOnly borrowedDate = DateOnly.FromDateTime(DateTime.Today);
        DateOnly returnDate = DateOnly.FromDateTime(ExpectedReturnDate.Value.DateTime);
        if (returnDate < borrowedDate)
        {
            StatusMessage = "Expected return date cannot be earlier than today.";
            return;
        }

        BorrowEquipmentResult result = await _borrowEquipmentService.BorrowAsync(
            new BorrowEquipmentRequest(
                SelectedStudent.Id,
                SelectedEquipment.Id,
                borrowedDate,
                returnDate));

        StatusMessage = result.Message;
        await LoadAsync();
    }
}
