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
            parent.AddEntity(newEntity);
        }
        else
        {
            _sceneService.ActiveScene?.Entities.Add(newEntity);
        }
    }

    [RelayCommand]
    private void DeleteEntity()
    {
        if (SelectedObject is GameEntity entity)
        {
            RemoveEntityRecursive(_sceneService.ActiveScene.Entities, entity);
        }
    }

    private bool RemoveEntityRecursive(IList<GameEntity> list, GameEntity target)
    {
        if (list.Remove(target)) return true;

        foreach (var entity in list)
        {
            if (RemoveEntityRecursive(entity.Children, target)) return true;
        }

        return false;
    }
}