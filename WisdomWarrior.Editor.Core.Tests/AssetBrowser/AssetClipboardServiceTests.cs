using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.AssetBrowser;

public class AssetClipboardServiceTests
{
    [Fact]
    public async Task CopyPathsAsync_DelegatesToBackend()
    {
        var backend = new FakeClipboardBackend
        {
            CopyResult = true
        };

        var service = new AssetClipboardService(backend);
        var paths = new[] { @"C:\Project\File.txt" };

        var result = await service.CopyPathsAsync(paths);

        Assert.True(result);
        Assert.Equal(paths, backend.CopiedPaths);
    }

    [Fact]
    public async Task GetClipboardPathsAsync_ReturnsPathsAndUpdatesMode()
    {
        var backend = new FakeClipboardBackend
        {
            ReadResult = new AssetClipboardReadResult(
                [@"C:\Project\File.txt", @"C:\Project\Folder"],
                CopyAssetMode.DuplicateExisting)
        };

        var service = new AssetClipboardService(backend);

        var paths = await service.GetClipboardPathsAsync();

        Assert.Equal(backend.ReadResult.Paths, paths);
        Assert.Equal(CopyAssetMode.DuplicateExisting, service.LastResolvedMode);
    }

    private sealed class FakeClipboardBackend : IAssetClipboardBackend
    {
        public bool CopyResult { get; set; }

        public IReadOnlyList<string> CopiedPaths { get; private set; } = [];

        public AssetClipboardReadResult ReadResult { get; set; } =
            new([], CopyAssetMode.ImportExternal);

        public Task<bool> CopyPathsAsync(IReadOnlyList<string> sourcePaths)
        {
            CopiedPaths = sourcePaths;
            return Task.FromResult(CopyResult);
        }

        public Task<AssetClipboardReadResult> ReadAsync()
        {
            return Task.FromResult(ReadResult);
        }
    }
}
