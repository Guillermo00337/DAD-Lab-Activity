namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public EquipmentViewModel BorrowEquipment { get; }
    public BorrowingsViewModel Borrowings { get; }
    public StudentsViewModel Students { get; }
    public EquipmentManagementViewModel EquipmentManagement { get; }
    public HistoryViewModel Histories { get; }

    public MainWindowViewModel(EquipmentViewModel borrowEquipment, BorrowingsViewModel borrowings,
        StudentsViewModel students, EquipmentManagementViewModel equipmentManagement, HistoryViewModel histories)
    {
        BorrowEquipment = borrowEquipment;
        Borrowings = borrowings;
        Students = students;
        EquipmentManagement = equipmentManagement;
        Histories = histories;
        _ = BorrowEquipment.LoadAsync();
        _ = Borrowings.LoadAsync();
        _ = Students.LoadAsync();
        _ = EquipmentManagement.LoadAsync();
        _ = Histories.LoadAsync();
    }
}
