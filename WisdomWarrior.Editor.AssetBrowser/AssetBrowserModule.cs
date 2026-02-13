using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.Core;
using AssetBrowserViewModel = WisdomWarrior.Editor.AssetBrowser.ViewModels.AssetBrowserViewModel;

namespace WisdomWarrior.Editor.AssetBrowser;

public class AssetBrowserModule : IEditorModule
{
    public void RegisterTypes(IServiceCollection services)
    {
        // Register your AssetBrowserViewModel here later
    }

    public void Initialize(IServiceProvider provider)
    {
        var shell = provider.GetRequiredService<IShell>();

        var view = new AssetBrowserViewModel();

        shell.Dock(DockLocation.Bottom, view, "Asset Browser");
    }
}