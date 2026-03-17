using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.AssetBrowser.Configurations;

public static class Configurations
{
    public static IServiceCollection ConfigureAssetBrowser(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAssetClipboardBackend, SystemAssetClipboardBackend>()
            .AddSingleton<IAssetClipboardService, AssetClipboardService>()
            .AddSingleton<IAssetClipboardActionService, AssetClipboardActionService>()
            .AddSingleton<AssetBrowserViewModel>()
            .AddSingleton<SolutionViewModel>()
            .AddSingleton<DirectoryViewModel>();
    }
}
