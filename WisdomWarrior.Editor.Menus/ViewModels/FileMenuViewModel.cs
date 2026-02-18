using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Menus.Views;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public partial class FileMenuViewModel(ProjectService projectService, WorkspaceService workspaceService) : ObservableObject
{
    [RelayCommand]
    private async Task OpenGameProject()
    {
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
}