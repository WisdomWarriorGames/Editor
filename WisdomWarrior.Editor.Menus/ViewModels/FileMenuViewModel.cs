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
    WorkspaceCreationService workspaceCreationService,
    WorkspaceLoader workspaceLoader,
    WorkspaceService workspaceService) : ObservableObject
{
    [RelayCommand]
    private async Task OpenGame()
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
        var workspace = workspaceLoader.Load(selectedFile);
        workspaceService.Load(workspace);
    }

    [RelayCommand]
    private async Task CreateNewGame()
    {
        var vm = new CreateProjectViewModel(workspaceCreationService, workspaceService);
        var view = new CreateProjectView { DataContext = vm };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await view.ShowDialog<bool>(desktop.MainWindow);
        }
    }
}
