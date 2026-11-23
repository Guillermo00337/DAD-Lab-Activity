namespace EquipmentBorrowing.Domain;

public sealed class Equipment
{
    public Equipment(int id, string assetTag, string name, bool isAvailable = true, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            throw new ArgumentException("Equipment identifier is required.", nameof(assetTag));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Id = id;
        AssetTag = assetTag.Trim();
        Name = name.Trim();
        IsAvailable = isAvailable;
        Description = NormalizeOptional(description);
    }

    public int Id { get; private set; }
    public string AssetTag { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; }

    public void UpdateDetails(string assetTag, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            throw new ArgumentException("Equipment identifier is required.", nameof(assetTag));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));
        AssetTag = assetTag.Trim();
        Name = name.Trim();
        Description = NormalizeOptional(description);
    }

    public void MarkBorrowed()
    {
        if (!IsAvailable) throw new InvalidOperationException("Equipment is already borrowed.");
        IsAvailable = false;
    }

    public void MarkAvailable() => IsAvailable = true;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
