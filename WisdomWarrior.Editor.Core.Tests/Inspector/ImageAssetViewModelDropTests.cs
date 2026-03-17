using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.Inspector.Models;
using WisdomWarrior.Engine.Core.Assets;

namespace WisdomWarrior.Editor.Core.Tests.Inspector;

public class ImageAssetViewModelDropTests
{
    [Fact]
    public async Task AcceptDrop_ValidAssetDropData_UpdatesAssetPath()
    {
        AssetPathContext.Configure(null, null);

        var imagePath = CreateTempFile(".png");
        ImageAsset? changedAsset = null;
        var viewModel = new ImageAssetViewModel(new ImageAsset(), asset => changedAsset = asset);

        var payload = new TestAssetDropData(imagePath, ".png", false);

        Assert.True(viewModel.AcceptDropCommand.CanExecute(payload));

        await viewModel.AcceptDropCommand.ExecuteAsync(payload);

        var expected = Path.GetFullPath(imagePath);
        Assert.Equal(expected, viewModel.Path);
        Assert.NotNull(changedAsset);
        Assert.Equal(expected, changedAsset!.AssetPath);
    }

    [Fact]
    public void AcceptDrop_InvalidExtension_IsRejected()
    {
        AssetPathContext.Configure(null, null);

        var viewModel = new ImageAssetViewModel(new ImageAsset(), _ => { });
        var payload = new TestAssetDropData("C:/temp/readme.txt", ".txt", false);

        Assert.False(viewModel.AcceptDropCommand.CanExecute(payload));
        Assert.Equal(string.Empty, viewModel.Path);
    }

    [Fact]
    public async Task AcceptDrop_MixedPayload_UsesFirstValidImageAsset()
    {
        AssetPathContext.Configure(null, null);

        var imagePath = CreateTempFile(".png");
        var viewModel = new ImageAssetViewModel(new ImageAsset(), _ => { });

        object payload = new object[]
        {
            new TestAssetDropData("C:/temp/readme.txt", ".txt", false),
            new TestAssetDropData(imagePath, ".png", false)
        };

        Assert.True(viewModel.AcceptDropCommand.CanExecute(payload));

        await viewModel.AcceptDropCommand.ExecuteAsync(payload);

        Assert.Equal(Path.GetFullPath(imagePath), viewModel.Path);
    }

    private static string CreateTempFile(string extension)
    {
        var root = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var path = Path.Combine(root, $"image{extension}");
        File.WriteAllBytes(path, [1, 2, 3, 4]);
        return path;
    }

    private sealed class TestAssetDropData(string fullPath, string extension, bool isFolder) : IAssetDropData
    {
        public string FullPath { get; } = fullPath;
        public string Extension { get; } = extension;
        public bool IsFolder { get; } = isFolder;
    }
}
