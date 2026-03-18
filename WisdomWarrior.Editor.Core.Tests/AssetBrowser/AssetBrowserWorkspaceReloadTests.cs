using System.Reflection;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.AssetBrowser;

public class AssetBrowserWorkspaceReloadTests
{
    [Fact]
    public void DirectoryViewModel_BindRegistry_WhenCalledTwiceWithSameRegistry_SubscribesOnce()
    {
        var registry = new FileSystemRegistry(new FileSystemService());
        var viewModel = new DirectoryViewModel(
            new FileSystemService(),
            new FakeAssetClipboardActionService(),
            new SelectionManager());

        InvokePrivateMethod(viewModel, "BindRegistry", registry);
        InvokePrivateMethod(viewModel, "BindRegistry", registry);

        Assert.Equal(1, GetEventHandlerCount(registry, "RegistryUpdated"));
    }

    [Fact]
    public void SolutionViewModel_BindRegistry_WhenCalledTwiceWithSameRegistry_SubscribesOncePerEvent()
    {
        var registry = new FileSystemRegistry(new FileSystemService());
        var workspaceService = new WorkspaceService(registry, new EditorManifestService());
        var viewModel = new SolutionViewModel(workspaceService, new FakeAssetClipboardActionService());

        InvokePrivateMethod(viewModel, "BindRegistry", registry);
        InvokePrivateMethod(viewModel, "BindRegistry", registry);

        Assert.Equal(1, GetEventHandlerCount(registry, "CurrentNodeChanged"));
        Assert.Equal(1, GetEventHandlerCount(registry, "FileSystemChanged"));
    }

    private static void InvokePrivateMethod(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(instance, args);
    }

    private static int GetEventHandlerCount(object instance, string eventName)
    {
        var field = instance.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var handler = field!.GetValue(instance) as Delegate;
        return handler?.GetInvocationList().Length ?? 0;
    }

    private sealed class FakeAssetClipboardActionService : IAssetClipboardActionService
    {
        public Task CopyPathsAsync(IReadOnlyList<string> sourcePaths)
        {
            return Task.CompletedTask;
        }

        public Task PasteIntoAsync(string targetDirectory)
        {
            return Task.CompletedTask;
        }
    }
}
