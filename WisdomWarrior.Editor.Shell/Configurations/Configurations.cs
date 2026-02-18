using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Menus.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;
using WisdomWarrior.Editor.SceneList.ViewModels;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell.Configurations;

public static class Configurations
{
    public static IServiceCollection Configure(this IServiceCollection services)
    {
        services.AddTransient<FileSystemService>();
        services.AddTransient<ProjectService>();

        services.AddSingleton<FileSystemRegistry>();
        services.AddSingleton<WorkspaceService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<AssetBrowserViewModel>();
        services.AddSingleton<MonoGameViewModel>();
        services.AddSingleton<SceneListViewModel>();
        services.AddSingleton<FileMenuViewModel>();

        return services;
    }
}