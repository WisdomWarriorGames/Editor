using System.Text.Json;
using System.Text.Json.Serialization;

namespace WisdomWarrior.Editor.FileSystem;

public class EditorSettingsService(IEditorSettingsPathProvider pathProvider)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public EditorSettings Load()
    {
        var settingsPath = pathProvider.GetSettingsFilePath();
        if (!File.Exists(settingsPath))
        {
            var defaultSettings = CreateDefault();
            Save(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            var settings = JsonSerializer.Deserialize<EditorSettings>(json, SerializerOptions);
            if (settings != null && Enum.IsDefined(settings.EditorTheme))
            {
                return settings;
            }
        }
        catch
        {
        }

        var fallbackSettings = CreateDefault();
        Save(fallbackSettings);
        return fallbackSettings;
    }

    public void Save(EditorSettings settings)
    {
        var settingsPath = pathProvider.GetSettingsFilePath();
        var settingsDirectory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(settingsPath, json);
    }

    public EditorSettings CreateDefault()
    {
        return new EditorSettings();
    }
}
