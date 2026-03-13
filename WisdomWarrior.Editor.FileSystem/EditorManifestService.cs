using System.Text.Json;

namespace WisdomWarrior.Editor.FileSystem;

public class EditorManifestService
{
    public const string FileName = "manifest.json";

    public string GetManifestPath(string rootPath)
    {
        return Path.Combine(rootPath, FileName);
    }

    public EditorManifest CreateDefault(string defaultProjectPath)
    {
        return new EditorManifest
        {
            DefaultProject = NormalizeRelativePath(defaultProjectPath),
            LastActiveScene = null
        };
    }

    public void Save(string rootPath, EditorManifest manifest)
    {
        var path = GetManifestPath(rootPath);
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }

    public EditorManifest? TryLoad(string rootPath)
    {
        var path = GetManifestPath(rootPath);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            var manifest = JsonSerializer.Deserialize<EditorManifest>(json);

            if (manifest == null)
            {
                return null;
            }

            manifest.DefaultProject = NormalizeRelativePath(manifest.DefaultProject);
            manifest.LastActiveScene = NormalizeNullableRelativePath(manifest.LastActiveScene);

            return manifest;
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizeRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path
            .Trim()
            .Replace('\\', '/');
    }

    private static string? NormalizeNullableRelativePath(string? path)
    {
        var normalized = NormalizeRelativePath(path);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
