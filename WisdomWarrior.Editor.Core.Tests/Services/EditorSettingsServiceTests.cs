using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class EditorSettingsServiceTests
{
    [Fact]
    public void Load_WhenSettingsFileMissing_CreatesDefaultSettingsFile()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var settingsPath = Path.Combine(tempRoot, "config", "editor-settings.json");
            var service = new EditorSettingsService(new FixedEditorSettingsPathProvider(settingsPath));

            var settings = service.Load();

            Assert.Equal(EditorTheme.Dark, settings.EditorTheme);
            Assert.True(File.Exists(settingsPath));

            var savedJson = File.ReadAllText(settingsPath);
            Assert.Contains("\"EditorTheme\": \"Dark\"", savedJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectory(tempRoot);
        }
    }

    [Fact]
    public void Save_PersistsThemeSelectionAcrossLoads()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var settingsPath = Path.Combine(tempRoot, "config", "editor-settings.json");
            var service = new EditorSettingsService(new FixedEditorSettingsPathProvider(settingsPath));

            service.Save(new EditorSettings
            {
                EditorTheme = EditorTheme.Light
            });

            var reloadedSettings = service.Load();

            Assert.Equal(EditorTheme.Light, reloadedSettings.EditorTheme);
        }
        finally
        {
            DeleteDirectory(tempRoot);
        }
    }

    [Fact]
    public void Load_WhenSettingsFileIsInvalid_FallsBackToDefaultAndRewritesFile()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var settingsPath = Path.Combine(tempRoot, "config", "editor-settings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            File.WriteAllText(settingsPath, "{ invalid json");

            var service = new EditorSettingsService(new FixedEditorSettingsPathProvider(settingsPath));

            var settings = service.Load();

            Assert.Equal(EditorTheme.Dark, settings.EditorTheme);

            var savedJson = File.ReadAllText(settingsPath);
            Assert.Contains("\"EditorTheme\": \"Dark\"", savedJson, StringComparison.Ordinal);
        }
        finally
        {
            DeleteDirectory(tempRoot);
        }
    }

    [Fact]
    public void PathProvider_UsesExpectedPlatformApplicationDataRoot()
    {
        var provider = new EditorSettingsPathProvider();

        var settingsPath = provider.GetSettingsFilePath();
        var expectedRoot = GetExpectedPlatformRoot();

        Assert.StartsWith(expectedRoot, settingsPath, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(
            Path.Combine("WisdomWarrior", "Editor", EditorSettingsPathProvider.SettingsFileName),
            settingsPath,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string GetExpectedPlatformRoot()
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

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private sealed class FixedEditorSettingsPathProvider(string settingsPath) : IEditorSettingsPathProvider
    {
        public string GetSettingsFilePath()
        {
            return settingsPath;
        }
    }
}
