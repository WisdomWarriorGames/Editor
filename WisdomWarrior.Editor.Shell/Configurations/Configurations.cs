using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.AssetBrowser.Configurations;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Menus.Services;
using WisdomWarrior.Editor.Inspector.ViewModels;
using WisdomWarrior.Editor.Menus.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;
using WisdomWarrior.Editor.SceneList.ViewModels;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell.Configurations;

public static class Configurations
{
    public static IServiceCollection Configure(this IServiceCollection services)
    {
        services.AddSingleton<FileSystemService>();
        services.AddSingleton<EditorManifestService>();
        services.AddSingleton<IEditorSettingsPathProvider, EditorSettingsPathProvider>();
        services.AddSingleton<EditorSettingsService>();
        services.AddTransient<WorkspaceCreationService>();
        services.AddTransient<WorkspaceLoader>();
        services.AddSingleton<ScenePersistenceService>();
        services.AddSingleton<ScenePathSynchronizationService>();

        services.AddSingleton<FileSystemRegistry>();
        services.AddSingleton<WorkspaceService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MonoGameViewModel>();
        services.AddSingleton<SceneHierarchyViewModel>();
        services.AddSingleton<FileMenuViewModel>();
        services.AddSingleton<InspectorViewModel>();
        services.AddSingleton<EditorThemeService>();

        services.AddSingleton<CurrentSceneManager>();
        services.AddSingleton<SelectionManager>();

        services.AddSingleton<EditorInputService>();

        services.ConfigureAssetBrowser();

        return services;
    }
}
