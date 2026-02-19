using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneListViewModel : ObservableObject
{
    private readonly CurrentSceneManager _sceneService;
    private readonly EditorContext _context;

    public object? SelectedObject
    {
        get => _context.SelectedEntity;
        set
        {
            if (value is GameEntity entity)
                _context.SelectedEntity = entity;
            else
                _context.SelectedEntity = null;
        }
    }

    public IEnumerable<Scene> SceneRoot => _sceneService.ActiveScene != null
        ? new[] { _sceneService.ActiveScene }
        : Array.Empty<Scene>();

    public SceneListViewModel(CurrentSceneManager sceneService, EditorContext context)
    {
        _sceneService = sceneService;
        _context = context;

        _sceneService.CurrentSceneReady += OnSceneReady;
    }

    private void OnSceneReady()
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