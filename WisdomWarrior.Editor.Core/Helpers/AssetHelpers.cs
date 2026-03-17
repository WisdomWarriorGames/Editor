using WisdomWarrior.Engine.Core.Assets;
using Size = WisdomWarrior.Engine.Core.DataTypes.Size;

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
            if (TryReadImageDimensions(resolvedPath, out var dimensions))
                asset.Dimensions = dimensions;
        }
        catch
        {
            asset.Dimensions = Size.Zero;
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

    private static bool TryReadImageDimensions(string path, out Size dimensions)
    {
        dimensions = Size.Zero;

        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        if (stream.Length < 10)
            return false;

        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != header.Length)
            return false;

        stream.Position = 0;

        if (header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47)
        {
            return TryReadPngDimensions(reader, out dimensions);
        }

        if (header[0] == 0xFF && header[1] == 0xD8)
        {
            return TryReadJpegDimensions(reader, out dimensions);
        }

        if (header[0] == 0x47 &&
            header[1] == 0x49 &&
            header[2] == 0x46)
        {
            return TryReadGifDimensions(reader, out dimensions);
        }

        if (header[0] == 0x42 && header[1] == 0x4D)
        {
            return TryReadBmpDimensions(reader, out dimensions);
        }

        return false;
    }

    private static bool TryReadPngDimensions(BinaryReader reader, out Size dimensions)
    {
        dimensions = Size.Zero;

        reader.BaseStream.Position = 16;
        if (reader.BaseStream.Length < 24)
            return false;

        var width = ReadInt32BigEndian(reader);
        var height = ReadInt32BigEndian(reader);
        dimensions = new Size(width, height);
        return width > 0 && height > 0;
    }

    private static bool TryReadGifDimensions(BinaryReader reader, out Size dimensions)
    {
        dimensions = Size.Zero;

        reader.BaseStream.Position = 6;
        if (reader.BaseStream.Length < 10)
            return false;

        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        dimensions = new Size(width, height);
        return width > 0 && height > 0;
    }

    private static bool TryReadBmpDimensions(BinaryReader reader, out Size dimensions)
    {
        dimensions = Size.Zero;

        reader.BaseStream.Position = 18;
        if (reader.BaseStream.Length < 26)
            return false;

        var width = reader.ReadInt32();
        var height = Math.Abs(reader.ReadInt32());
        dimensions = new Size(width, height);
        return width > 0 && height > 0;
    }

    private static bool TryReadJpegDimensions(BinaryReader reader, out Size dimensions)
    {
        dimensions = Size.Zero;
        reader.BaseStream.Position = 2;

        while (reader.BaseStream.Position + 4 <= reader.BaseStream.Length)
        {
            if (reader.ReadByte() != 0xFF)
                return false;

            byte marker;
            do
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    return false;

                marker = reader.ReadByte();
            }
            while (marker == 0xFF);

            if (marker == 0xD9 || marker == 0xDA)
                return false;

            var segmentLength = ReadUInt16BigEndian(reader);
            if (segmentLength < 2 || reader.BaseStream.Position + segmentLength - 2 > reader.BaseStream.Length)
                return false;

            if (IsJpegStartOfFrame(marker))
            {
                reader.ReadByte();
                var height = ReadUInt16BigEndian(reader);
                var width = ReadUInt16BigEndian(reader);
                dimensions = new Size(width, height);
                return width > 0 && height > 0;
            }

            reader.BaseStream.Position += segmentLength - 2;
        }

        return false;
    }

    private static bool IsJpegStartOfFrame(byte marker)
    {
        return marker switch
        {
            0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF => true,
            _ => false
        };
    }

    private static int ReadInt32BigEndian(BinaryReader reader)
    {
        Span<byte> buffer = stackalloc byte[4];
        reader.Read(buffer);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();

        return BitConverter.ToInt32(buffer);
    }

    private static ushort ReadUInt16BigEndian(BinaryReader reader)
    {
        Span<byte> buffer = stackalloc byte[2];
        reader.Read(buffer);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();

        return BitConverter.ToUInt16(buffer);
    }
}
