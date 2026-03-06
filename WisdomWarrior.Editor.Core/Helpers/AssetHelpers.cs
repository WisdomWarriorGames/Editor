namespace WisdomWarrior.Editor.Core.Helpers;

public class AssetHelpers
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif"
    };

    public static bool IsImage(string extension) => SupportedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
}