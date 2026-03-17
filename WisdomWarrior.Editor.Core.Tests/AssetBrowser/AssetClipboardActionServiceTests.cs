using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.AssetBrowser;

public class AssetClipboardActionServiceTests
{
    [Fact]
    public async Task PasteIntoAsync_WhenClipboardIsEmpty_DoesNothing()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var clipboardService = new FakeClipboardService([], CopyAssetMode.ImportExternal);
            var actionService = new AssetClipboardActionService(clipboardService, new FileSystemService());

            await actionService.PasteIntoAsync(tempRoot);

            Assert.Empty(Directory.EnumerateFileSystemEntries(tempRoot));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task PasteIntoAsync_CopiesValidClipboardItemsAndSkipsMissingOnes()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourceRoot = Path.Combine(tempRoot, "Source");
            var targetRoot = Path.Combine(tempRoot, "Target");
            Directory.CreateDirectory(sourceRoot);
            Directory.CreateDirectory(targetRoot);

            var sourceFile = Path.Combine(sourceRoot, "Config.json");
            File.WriteAllText(sourceFile, "{ }");

            var clipboardService = new FakeClipboardService(
                [sourceFile, Path.Combine(sourceRoot, "Missing.json")],
                CopyAssetMode.DuplicateExisting);
            var actionService = new AssetClipboardActionService(clipboardService, new FileSystemService());

            await actionService.PasteIntoAsync(targetRoot);

            Assert.True(File.Exists(Path.Combine(targetRoot, "Config.json")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private sealed class FakeClipboardService : IAssetClipboardService
    {
        private readonly IReadOnlyList<string> _paths;

        public FakeClipboardService(IReadOnlyList<string> paths, CopyAssetMode mode)
        {
            _paths = paths;
            LastResolvedMode = mode;
        }

        public CopyAssetMode LastResolvedMode { get; private set; }

        public Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths)
        {
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<string>> GetClipboardPathsAsync()
        {
            return Task.FromResult(_paths);
        }
    }
}
