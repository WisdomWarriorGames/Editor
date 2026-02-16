using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell.Configurations;

public static class Configurations
{
    public static IServiceCollection Configure(this IServiceCollection services)
    {
        services.AddTransient<FileSystemService>();
        services.AddSingleton<FileSystemRegistry>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<AssetBrowserViewModel>();

        return services;
    }
}