using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public partial class CreateSlnxProjectViewModel(SlnxProjectService projectService, WorkspaceService workspaceService) : ObservableObject
{
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _gameName = "MyNewGame";
    [ObservableProperty] private string _projectPath = string.Empty;

    [RelayCommand]
    private async Task BrowseFolder(Window parent)
    {
        var folders = await parent.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Project Location",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            ProjectPath = folders[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private async Task Confirm(Window window)
    {
        if (string.IsNullOrWhiteSpace(GameName) || !Directory.Exists(ProjectPath))
            return;

        IsLoading = true;

        await Task.Run(() =>
        {
            var workspace = projectService.CreateSolution(ProjectPath, GameName);
            workspaceService.Load(workspace);
        });

        IsLoading = false;
        window.Close(true);
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.Close(false);
    }
}
