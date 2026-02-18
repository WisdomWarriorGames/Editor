using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.SceneList.Services;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneListViewModel : ObservableObject
{
    private readonly SceneService _sceneService;
    private readonly WorkspaceService _workspaceService;

    [ObservableProperty] private object? _selectedObject;

    public IEnumerable<Scene> SceneRoot => _sceneService.ActiveScene != null
        ? new[] { _sceneService.ActiveScene }
        : Array.Empty<Scene>();

    public SceneListViewModel(SceneService sceneService, WorkspaceService workspaceService)
    {
        _sceneService = sceneService;
        _workspaceService = workspaceService;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(Initialize);
    }

    private void Initialize()
    {
        _sceneService.ActiveScene?.Entities.Add(
            new GameEntity
            {
                Name = "New Entity",
                Children =
                [
                    new GameEntity
                    {
                        Name = "Child",
                    }
                ]
            }
        );

        OnPropertyChanged(nameof(SceneRoot));
    }

    [RelayCommand]
    private void CreateNewEntity()
    {
        var newEntity = new GameEntity { Name = "New Entity" };

        if (SelectedObject is Scene scene)
        {
            scene.Entities.Add(newEntity);
        }
        else if (SelectedObject is GameEntity parent)
        {
            parent.Children.Add(newEntity);
        }
        else
        {
            _sceneService.ActiveScene?.Entities.Add(newEntity);
        }
    }
}