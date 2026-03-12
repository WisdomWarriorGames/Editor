using SixLabors.ImageSharp;
using WisdomWarrior.Engine.Core.Assets;
using Size = System.Drawing.Size;

namespace WisdomWarrior.Editor.Core.Helpers;

public static class AssetHelpers
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif"
    };

    public static bool IsImage(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        return SupportedImageExtensions.Contains(extension);
    }

    public static ImageAsset CreateImageAsset(string fullPath)
    {
        var asset = new ImageAsset
        {
            AssetPath = fullPath
        };

        if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            return asset;

        try
        {
            var info = Image.Identify(fullPath);

            if (info != null)
            {
                asset.Dimensions = new Engine.Core.DataTypes.Size(info.Width, info.Height);
            }
        }
        catch
        {
            asset.Dimensions = Engine.Core.DataTypes.Size.Zero;
        }

        return asset;
    }
}