using EquipmentBorrowing.Domain;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class EquipmentTests
{
    [Fact]
    public void Constructor_DefaultAvailability_IsTrue()
    {
        var equipment = new Equipment(1, "MIC-001", "Wireless Microphone");

        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public void MarkBorrowed_AvailableEquipment_BecomesUnavailable()
    {
        var equipment = new Equipment(1, "MIC-001", "Wireless Microphone");

        equipment.MarkBorrowed();

        Assert.False(equipment.IsAvailable);
    }

    [Fact]
    public void MarkBorrowed_UnavailableEquipment_ThrowsException()
    {
        var equipment = new Equipment(
            1, "MIC-001", "Wireless Microphone", isAvailable: false);

        Assert.Throws<InvalidOperationException>(() =>
            equipment.MarkBorrowed());
    }

    [Fact]
    public void MarkAvailable_BorrowedEquipment_BecomesAvailable()
    {
        var equipment = new Equipment(1, "MIC-001", "Wireless Microphone");
        equipment.MarkBorrowed();

        equipment.MarkAvailable();

        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public void UpdateDetails_TrimsEquipmentInformation()
    {
        var equipment = new Equipment(1, "MIC-001", "Wireless Microphone");

        equipment.UpdateDetails(" CAM-002 ", " Camera ", "  Digital camera  ");

        Assert.Equal("CAM-002", equipment.AssetTag);
        Assert.Equal("Camera", equipment.Name);
        Assert.Equal("Digital camera", equipment.Description);
    }

    [Fact]
    public void Constructor_EmptyAssetTag_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Equipment(1, " ", "Wireless Microphone"));
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Equipment(1, "MIC-001", " "));
    }
}
