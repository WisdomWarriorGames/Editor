namespace WisdomWarrior.Editor.FileSystem;

public class EditorSettingsPathProvider : IEditorSettingsPathProvider
{
    public const string SettingsFileName = "editor-settings.json";

    public string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsDirectoryPath(), SettingsFileName);
    }

    private static string GetSettingsDirectoryPath()
    {
        var basePath = GetBaseApplicationDataPath();
        return Path.Combine(basePath, "WisdomWarrior", "Editor");
    }

    private static string GetBaseApplicationDataPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library",
                "Application Support");
        }

        var xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (!string.IsNullOrWhiteSpace(xdgConfigHome))
        {
            return xdgConfigHome;
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            ".config");
    }
}
