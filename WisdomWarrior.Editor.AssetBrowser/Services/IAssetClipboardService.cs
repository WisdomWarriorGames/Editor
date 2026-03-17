using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.Services;

public interface IAssetClipboardService
{
    CopyAssetMode LastResolvedMode { get; }

    Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths);

    Task<IReadOnlyList<string>> GetClipboardPathsAsync();
}
