using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.Services;

public sealed class AssetClipboardActionService : IAssetClipboardActionService
{
    private readonly IAssetClipboardService _clipboardService;
    private readonly FileSystemService _fileSystemService;

    public AssetClipboardActionService(IAssetClipboardService clipboardService, FileSystemService fileSystemService)
    {
        _clipboardService = clipboardService;
        _fileSystemService = fileSystemService;
    }

    public async Task CopyPathsAsync(IReadOnlyList<string> sourcePaths)
    {
        if (sourcePaths.Count == 0)
        {
            return;
        }

        var success = await _clipboardService.CopyPathsAsync(sourcePaths);
        if (!success)
        {
            QueueToast("Could not copy the selected items.");
        }
    }

    public async Task PasteIntoAsync(string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
        {
            return;
        }

        var sourcePaths = await _clipboardService.GetClipboardPathsAsync();
        if (sourcePaths.Count == 0)
        {
            return;
        }

        var mode = _clipboardService.LastResolvedMode;
        var validPaths = new List<string>();
        var skippedCount = 0;

        foreach (var sourcePath in sourcePaths)
        {
            if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            {
                skippedCount++;
                continue;
            }

            if (Directory.Exists(sourcePath) && IsInvalidPasteTarget(sourcePath, targetDirectory))
            {
                skippedCount++;
                continue;
            }

            validPaths.Add(sourcePath);
        }

        if (validPaths.Count == 0)
        {
            if (skippedCount > 0)
            {
                QueueToast("No clipboard items could be pasted here.");
            }

            return;
        }

        ISukiToast? loadingToast = null;
        var progress = new Progress<string>(message =>
        {
            if (loadingToast != null)
            {
                loadingToast.Content = message;
            }
        });

        loadingToast = TryCreateLoadingToast("Pasting Assets");
        await _fileSystemService.CopyAssetsAsync(validPaths, targetDirectory, mode, progress);
        DismissToast(loadingToast);

        if (skippedCount > 0)
        {
            QueueToast("Some clipboard items could not be pasted.");
        }
    }

    private static bool IsInvalidPasteTarget(string sourceDirectory, string targetDirectory)
    {
        var fullSourcePath = Path.GetFullPath(sourceDirectory);
        var fullTargetPath = Path.GetFullPath(targetDirectory);

        if (string.Equals(fullSourcePath, fullTargetPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var relativePath = Path.GetRelativePath(fullSourcePath, fullTargetPath);
        return !relativePath.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relativePath);
    }

    private static ISukiToast? TryCreateLoadingToast(string title)
    {
        try
        {
            return EditorUI.ToastManager.CreateToast()
                .WithTitle(title)
                .WithLoadingState(true)
                .Queue();
        }
        catch
        {
            return null;
        }
    }

    private static void DismissToast(ISukiToast? toast)
    {
        if (toast == null)
        {
            return;
        }

        try
        {
            EditorUI.ToastManager.Dismiss(toast);
        }
        catch
        {
        }
    }

    private static void QueueToast(string title)
    {
        try
        {
            EditorUI.ToastManager.CreateToast()
                .WithTitle(title)
                .Queue();
        }
        catch
        {
        }
    }
}
