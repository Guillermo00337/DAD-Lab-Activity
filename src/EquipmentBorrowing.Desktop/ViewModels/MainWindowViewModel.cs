using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly EquipmentViewModel _equipmentViewModel;
    private readonly BorrowingsViewModel _borrowingsViewModel;

    public MainWindowViewModel(
        EquipmentViewModel equipmentViewModel,
        BorrowingsViewModel borrowingsViewModel)
    {
        _equipmentViewModel = equipmentViewModel;
        _borrowingsViewModel = borrowingsViewModel;
        CurrentViewModel = _equipmentViewModel;
        _ = _equipmentViewModel.LoadAsync();
    }

    public ViewModelBase CurrentViewModel { get; private set; }

    [RelayCommand]
    private async Task ShowEquipmentAsync()
    {
        CurrentViewModel = _equipmentViewModel;
        OnPropertyChanged(nameof(CurrentViewModel));
        await _equipmentViewModel.LoadAsync();
    }

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        CurrentViewModel = _borrowingsViewModel;
        OnPropertyChanged(nameof(CurrentViewModel));
        await _borrowingsViewModel.LoadAsync();
    }
}
