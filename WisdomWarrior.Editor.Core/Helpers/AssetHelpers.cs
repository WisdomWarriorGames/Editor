using SixLabors.ImageSharp;
using WisdomWarrior.Engine.Core.Assets;

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

    public static ImageAsset CreateImageAsset(string path)
    {
        var normalizedPath = NormalizeAssetPathForStorage(path);
        var resolvedPath = ResolveAbsoluteAssetPath(normalizedPath);

        var asset = new ImageAsset
        {
            AssetPath = normalizedPath
        };

        if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
            return asset;

        try
        {
            var info = Image.Identify(resolvedPath);

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

    public static string NormalizeAssetPathForStorage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var trimmedPath = path.Trim();

        if (!Path.IsPathRooted(trimmedPath))
            return NormalizeRelative(trimmedPath);

        var absolutePath = Path.GetFullPath(trimmedPath);
        var projectRoot = AssetPathContext.ProjectRoot;

        if (string.IsNullOrWhiteSpace(projectRoot))
            return absolutePath;

        if (IsUnderRoot(absolutePath, projectRoot))
            return ToProjectRelative(projectRoot, absolutePath);

        var importedAbsolutePath = TryImportIntoProject(absolutePath);
        if (!string.IsNullOrWhiteSpace(importedAbsolutePath))
            return ToProjectRelative(projectRoot, importedAbsolutePath);

        return string.Empty;
    }

    public static string ResolveAbsoluteAssetPath(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return string.Empty;

        if (Path.IsPathRooted(storedPath))
            return Path.GetFullPath(storedPath);

        var projectRoot = AssetPathContext.ProjectRoot;
        if (string.IsNullOrWhiteSpace(projectRoot))
            return Path.GetFullPath(storedPath);

        var normalizedRelative = storedPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(projectRoot, normalizedRelative));
    }

    private static string? TryImportIntoProject(string absolutePath)
    {
        if (!File.Exists(absolutePath))
            return null;

        var importRoot = AssetPathContext.AssetImportRoot ?? AssetPathContext.ProjectRoot;
        if (string.IsNullOrWhiteSpace(importRoot))
            return null;

        var importDirectory = Path.Combine(importRoot, "ImportedAssets", "Images");
        Directory.CreateDirectory(importDirectory);

        var fileName = Path.GetFileName(absolutePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var candidatePath = Path.Combine(importDirectory, fileName);
        var sourceFullPath = Path.GetFullPath(absolutePath);

        if (StringComparer.OrdinalIgnoreCase.Equals(sourceFullPath, Path.GetFullPath(candidatePath)))
            return candidatePath;

        var destinationPath = EnsureUniqueDestination(candidatePath);
        File.Copy(sourceFullPath, destinationPath, overwrite: false);
        return destinationPath;
    }

    private static string EnsureUniqueDestination(string candidatePath)
    {
        if (!File.Exists(candidatePath))
            return candidatePath;

        var directory = Path.GetDirectoryName(candidatePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(candidatePath);
        var extension = Path.GetExtension(candidatePath);

        var index = 1;
        while (true)
        {
            var uniquePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{index}{extension}");
            if (!File.Exists(uniquePath))
                return uniquePath;

            index++;
        }
    }

    private static bool IsUnderRoot(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);

        return !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }

    private static string ToProjectRelative(string projectRoot, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(projectRoot, absolutePath);
        return NormalizeRelative(relativePath);
    }

    private static string NormalizeRelative(string path)
    {
        var normalized = path.Replace('\\', '/');

        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized.Substring(2);
        }

        return normalized;
    }
}
