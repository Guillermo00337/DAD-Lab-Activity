using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class StudentsViewModel : ViewModelBase
{
    private readonly BorrowingApiClient _api;

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private string studentNumber = string.Empty;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string contactInformation = string.Empty;
    [ObservableProperty] private bool isEligible = true;
    [ObservableProperty] private string statusMessage = "Register or select a student to edit.";
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Student> Students { get; } = [];

    public StudentsViewModel(BorrowingApiClient api) => _api = api;

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is null) return;
        StudentNumber = value.StudentNumber;
        FullName = value.FullName;
        ContactInformation = value.ContactInformation ?? string.Empty;
        IsEligible = value.IsAllowedToBorrow;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        int? selectedId = SelectedStudent?.Id;
        IsLoading = true;
        try
        {
            Students.Clear();
            foreach (var item in await _api.GetStudentsAsync()) Students.Add(item);
            SelectedStudent = selectedId.HasValue
                ? Students.FirstOrDefault(x => x.Id == selectedId.Value)
                : null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to load students from the API: {ex.GetBaseException().Message}";
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
            var message = await _api.RegisterStudentAsync(
                StudentNumber, FullName, ContactInformation);
            ClearForm();
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Student registration failed: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student first.";
            return;
        }

        try
        {
            var message = await _api.UpdateStudentAsync(
                SelectedStudent.Id, StudentNumber, FullName, ContactInformation);
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Student update failed: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand]
    private async Task SaveEligibilityAsync()
    {
        if (SelectedStudent is null)
        {
            StatusMessage = "Select a student first.";
            return;
        }

        try
        {
            var message = await _api.SetStudentEligibilityAsync(
                SelectedStudent.Id, IsEligible);
            await LoadAsync();
            StatusMessage = message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Eligibility update failed: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedStudent = null;
        StudentNumber = FullName = ContactInformation = string.Empty;
        IsEligible = true;
    }
}