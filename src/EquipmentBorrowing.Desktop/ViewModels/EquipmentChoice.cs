using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Desktop.ViewModels;

public sealed partial class EquipmentChoice : ObservableObject
{
    public EquipmentChoice(Equipment equipment) => Equipment = equipment;
    public Equipment Equipment { get; }
    public int Id => Equipment.Id;
    public string DisplayName => $"{Equipment.AssetTag} — {Equipment.Name}";
    public bool IsAvailable => Equipment.IsAvailable;
    [ObservableProperty] private bool isSelected;
}
