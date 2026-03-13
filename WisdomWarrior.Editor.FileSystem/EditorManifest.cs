using System.Text.Json.Serialization;

namespace WisdomWarrior.Editor.FileSystem;

public class EditorManifest
{
    [JsonPropertyName("defaultProject")]
    public string DefaultProject { get; set; } = string.Empty;

    [JsonPropertyName("lastActiveScene")]
    public string? LastActiveScene { get; set; }
}
