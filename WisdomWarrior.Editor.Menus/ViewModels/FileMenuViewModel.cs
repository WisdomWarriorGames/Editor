using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Menus.Views;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public partial class FileMenuViewModel(
    ProjectService projectService,
    SlnxProjectService slnxProjectService,
    SlnxWorkspaceLoader slnxWorkspaceLoader,
    WorkspaceService workspaceService) : ObservableObject
{
    [RelayCommand]
    private async Task OpenGameProject()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;
            
            var manifestFilter = new FilePickerFileType("Wisdom Warrior Manifest")
            {
                Patterns = new[] { "*.manifest.json" }
            };
            
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Wisdom Warrior Project",
                AllowMultiple = false,
                FileTypeFilter = new[] { manifestFilter }
            });
            
            if (files.Count > 0)
            {
                var selectedFile = files[0].Path.LocalPath;
                
                workspaceService.Load(selectedFile);
            }
        }
    }

    [RelayCommand]
    private async Task OpenGameSolution()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        if (topLevel == null) return;

        var solutionFilter = new FilePickerFileType("Solution (slnx)")
        {
            Patterns = new[] { "*.slnx" }
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Solution",
            AllowMultiple = false,
            FileTypeFilter = new[] { solutionFilter }
        });

        if (files.Count <= 0)
        {
            return;
        }

        var selectedFile = files[0].Path.LocalPath;
        var workspace = slnxWorkspaceLoader.Load(selectedFile);
        workspaceService.Load(workspace);
    }

    [RelayCommand]
    private async Task CreateNewGame()
    {
        var vm = new CreateProjectViewModel(projectService, workspaceService);
        var view = new CreateProjectView { DataContext = vm };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await view.ShowDialog<bool>(desktop.MainWindow);
        }
    }

    [RelayCommand]
    private async Task CreateNewGameSlnx()
    {
        var vm = new CreateSlnxProjectViewModel(slnxProjectService, workspaceService);
        var view = new CreateProjectView { DataContext = vm };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await view.ShowDialog<bool>(desktop.MainWindow);
        }
    }
}
