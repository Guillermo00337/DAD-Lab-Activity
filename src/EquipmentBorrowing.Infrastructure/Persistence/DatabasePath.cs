namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabasePath
{
    public static string GetDatabasePath()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EquipmentBorrowing");

        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "EquipmentBorrowing.db");
    }
}
