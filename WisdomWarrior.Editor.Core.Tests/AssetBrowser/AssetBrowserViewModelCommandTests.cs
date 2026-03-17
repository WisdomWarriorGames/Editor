using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.Core.Tests.AssetBrowser;

public class AssetBrowserViewModelCommandTests
{
    [Fact]
    public async Task DirectoryCopySelected_UsesAllSelectedAssetPaths()
    {
        var actionService = new FakeAssetClipboardActionService();
        var viewModel = new DirectoryViewModel(new FileSystemService(), actionService, new SelectionManager());
        var tempRoot = CreateTempRoot();

        try
        {
            viewModel.SelectedItems.Add(new AssetViewModel(
                Path.Combine(tempRoot, "FolderA"),
                "FolderA",
                new FileSystemService(),
                actionService,
                _ => { }));
            viewModel.SelectedItems.Add(new AssetViewModel(
                Path.Combine(tempRoot, "FolderB"),
                "FolderB",
                new FileSystemService(),
                actionService,
                _ => { }));

            await viewModel.CopySelectedCommand.ExecuteAsync(null);

            Assert.Equal(
                [Path.Combine(tempRoot, "FolderA"), Path.Combine(tempRoot, "FolderB")],
                actionService.CopiedBatches.Single());
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task DirectoryPasteHere_TargetsCurrentNodePath()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            registry = new FileSystemRegistry(new FileSystemService());
            registry.Initialize(tempRoot);

            var actionService = new FakeAssetClipboardActionService();
            var viewModel = new DirectoryViewModel(new FileSystemService(), actionService, new SelectionManager());
            SetPrivateField(viewModel, "_registry", registry);

            await viewModel.PasteHereCommand.ExecuteAsync(null);

            Assert.Equal([tempRoot], actionService.PasteTargets);
        }
        finally
        {
            registry?.Dispose();
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task AssetCommands_CopyAndPasteInto_UseAssetPaths()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var folderPath = Path.Combine(tempRoot, "Textures");
            Directory.CreateDirectory(folderPath);

            var actionService = new FakeAssetClipboardActionService();
            var viewModel = new AssetViewModel(
                new FileSystemNode(folderPath, true),
                new FileSystemService(),
                actionService,
                new SelectionManager());

            await viewModel.CopyCommand.ExecuteAsync(null);
            await viewModel.PasteIntoCommand.ExecuteAsync(null);

            Assert.Equal([folderPath], actionService.CopiedBatches.Single());
            Assert.Equal([folderPath], actionService.PasteTargets);
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task BreadcrumbPasteHere_TargetsBreadcrumbPath()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            registry = new FileSystemRegistry(new FileSystemService());
            registry.Initialize(tempRoot);
            var actionService = new FakeAssetClipboardActionService();
            var breadcrumb = new BreadcrumbViewModel("Root", tempRoot, new FileSystemService(), actionService, registry);

            await breadcrumb.PasteHereCommand.ExecuteAsync(null);

            Assert.Equal([tempRoot], actionService.PasteTargets);
        }
        finally
        {
            registry?.Dispose();
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task SolutionTreeCommands_CopyFoldersAndIgnoreFiles()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var actionService = new FakeAssetClipboardActionService();
            var workspaceService = new WorkspaceService(new FileSystemRegistry(new FileSystemService()), new EditorManifestService());
            var viewModel = new SolutionViewModel(workspaceService, actionService);

            var folderNode = new FileSystemNode(Path.Combine(tempRoot, "Levels"), true);
            var fileNode = new FileSystemNode(Path.Combine(tempRoot, "Game.scene.json"), false);

            await viewModel.CopyFolderCommand.ExecuteAsync(folderNode);
            await viewModel.CopyFolderCommand.ExecuteAsync(fileNode);
            await viewModel.PasteIntoFolderCommand.ExecuteAsync(folderNode);

            Assert.Single(actionService.CopiedBatches);
            Assert.Equal([folderNode.FullPath], actionService.CopiedBatches.Single());
            Assert.Equal([folderNode.FullPath], actionService.PasteTargets);
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(instance, value);
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private sealed class FakeAssetClipboardActionService : IAssetClipboardActionService
    {
        public List<IReadOnlyList<string>> CopiedBatches { get; } = [];

        public List<string> PasteTargets { get; } = [];

        public Task CopyPathsAsync(IReadOnlyList<string> sourcePaths)
        {
            CopiedBatches.Add(sourcePaths);
            return Task.CompletedTask;
        }

        public Task PasteIntoAsync(string targetDirectory)
        {
            PasteTargets.Add(targetDirectory);
            return Task.CompletedTask;
        }
    }
}
