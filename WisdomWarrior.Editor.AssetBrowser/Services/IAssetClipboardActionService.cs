namespace WisdomWarrior.Editor.AssetBrowser.Services;

public interface IAssetClipboardActionService
{
    Task CopyPathsAsync(IReadOnlyList<string> sourcePaths);

    Task PasteIntoAsync(string targetDirectory);
}
