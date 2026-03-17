using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.Services;

public sealed class AssetClipboardService : IAssetClipboardService
{
    private readonly IAssetClipboardBackend _backend;

    public AssetClipboardService(IAssetClipboardBackend backend)
    {
        _backend = backend;
    }

    public CopyAssetMode LastResolvedMode { get; private set; } = CopyAssetMode.ImportExternal;

    public Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths)
    {
        return _backend.CopyPathsAsync(sourcePaths);
    }

    public async Task<IReadOnlyList<string>> GetClipboardPathsAsync()
    {
        var result = await _backend.ReadAsync();
        LastResolvedMode = result.Mode;
        return result.Paths;
    }
}

public interface IAssetClipboardBackend
{
    Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths);

    Task<AssetClipboardReadResult> ReadAsync();
}

public sealed record AssetClipboardReadResult(IReadOnlyList<string> Paths, CopyAssetMode Mode);

internal sealed class SystemAssetClipboardBackend : IAssetClipboardBackend
{
    private const string InternalClipboardFormat = "WisdomWarrior.AssetBrowser.CopySource";
    private const string InternalClipboardMarker = "Editor";

    public async Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths)
    {
        var clipboardContext = GetClipboardContext();
        if (clipboardContext == null)
        {
            return false;
        }

        var storageItems = new List<IStorageItem>();
        foreach (var sourcePath in sourcePaths)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                continue;
            }

            IStorageItem? storageItem = null;
            if (Directory.Exists(sourcePath))
            {
                storageItem = await clipboardContext.StorageProvider.TryGetFolderFromPathAsync(sourcePath);
            }
            else if (File.Exists(sourcePath))
            {
                storageItem = await clipboardContext.StorageProvider.TryGetFileFromPathAsync(sourcePath);
            }

            if (storageItem != null)
            {
                storageItems.Add(storageItem);
            }
        }

        if (storageItems.Count == 0)
        {
            return false;
        }

        var dataObject = new DataObject();
        dataObject.Set(DataFormats.Files, storageItems);
        dataObject.Set(InternalClipboardFormat, InternalClipboardMarker);

        await clipboardContext.Clipboard.SetDataObjectAsync(dataObject);
        await clipboardContext.Clipboard.FlushAsync();

        return true;
    }

    public async Task<AssetClipboardReadResult> ReadAsync()
    {
        var clipboardContext = GetClipboardContext();
        if (clipboardContext == null)
        {
            return new AssetClipboardReadResult([], CopyAssetMode.ImportExternal);
        }

        var mode = await ResolveCopyModeAsync(clipboardContext.Clipboard);
        var storageItems = await clipboardContext.Clipboard.TryGetFilesAsync();
        if (storageItems == null)
        {
            return new AssetClipboardReadResult([], mode);
        }

        var localPaths = storageItems
            .Select(item => item.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToArray();

        return new AssetClipboardReadResult(localPaths, mode);
    }

    private static async Task<CopyAssetMode> ResolveCopyModeAsync(IClipboard clipboard)
    {
        var marker = await clipboard.GetDataAsync(InternalClipboardFormat);
        return marker is string value && string.Equals(value, InternalClipboardMarker, StringComparison.Ordinal)
            ? CopyAssetMode.DuplicateExisting
            : CopyAssetMode.ImportExternal;
    }

    private static ClipboardContext? GetClipboardContext()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow == null)
        {
            return null;
        }

        var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        if (topLevel == null)
        {
            return null;
        }

        return new ClipboardContext(topLevel.Clipboard, topLevel.StorageProvider);
    }

    private sealed record ClipboardContext(IClipboard Clipboard, IStorageProvider StorageProvider);
}

