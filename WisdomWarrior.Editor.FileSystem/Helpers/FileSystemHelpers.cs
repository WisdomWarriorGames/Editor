using Avalonia.Platform.Storage;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;

namespace WisdomWarrior.Editor.FileSystem.Helpers;

public static class FileSystemHelpers
{
    public static void ProcessFileSystemDropAsync(
        this object? droppedItem,
        string destinationPath,
        FileSystemService fileSystem)
    {
        if (droppedItem == null || fileSystem == null) return;

        ISukiToast? loadingToast = null;
        var progress = new Progress<string>(message =>
        {
            if (loadingToast != null)
            {
                loadingToast.Content = message;
            }
        });

        var workQueue = droppedItem.GetPayloadItems<IDroppableAsset>().ToList();

        var externalPaths = new List<string>();
        if (droppedItem is IStorageItem file)
        {
            var path = file.TryGetLocalPath();
            if (path != null) externalPaths.Add(path);
        }
        else if (droppedItem is IEnumerable<IStorageItem> files)
        {
            foreach (var f in files)
            {
                var path = f.TryGetLocalPath();
                if (path != null) externalPaths.Add(path);
            }
        }

        if (workQueue.Count == 0 && externalPaths.Count == 0) return;

        _ = Task.Run(async () =>
        {
            await Task.Delay(100);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var title = externalPaths.Any() ? "Importing Assets" : "Moving Assets";
                loadingToast = EditorUI.ToastManager.CreateToast()
                    .WithTitle(title)
                    .WithLoadingState(true)
                    .Queue();
            });

            foreach (var path in externalPaths)
            {
                await fileSystem.CopyAsset(destinationPath, path, progress);
            }

            foreach (var item in workQueue)
            {
                await item.ProcessAsync(destinationPath, progress);
            }

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { EditorUI.ToastManager.Dismiss(loadingToast); });
        });
    }
}
