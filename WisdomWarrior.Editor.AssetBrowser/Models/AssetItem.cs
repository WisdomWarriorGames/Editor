namespace WisdomWarrior.Editor.AssetBrowser.Models;

public class AssetItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public string Extension { get; set; } = string.Empty;

    // Logic to determine icon can go here or in a converter
    public string Icon => IsFolder ? "📁" : GetFileIcon(Extension);

    private string GetFileIcon(string ext) => ext switch
    {
        ".png" or ".jpg" => "🖼️",
        ".cs" => "📝",
        ".audio" => "🎵",
        _ => "📄"
    };
}