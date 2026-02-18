using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Threading;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.Services;

public class SceneService
{
    private bool _isDirty;
    private readonly DispatcherTimer _saveTimer;

    private readonly WorkspaceService _workspaceService;

    public Scene ActiveScene { get; set; }

    public SceneService(WorkspaceService workspaceService, FileSystemService fileSystemService)
    {
        _workspaceService = workspaceService;
        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;

        _saveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _saveTimer.Tick += (s, e) => CheckAndSave();
        _saveTimer.Start();
    }

    public void SaveScene()
    {
        var json = JsonSerializer.Serialize(ActiveScene, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_workspaceService.ActiveScene, json);
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        if (_workspaceService.ActiveScene != null)
        {
            var json = File.ReadAllText(_workspaceService.ActiveScene);
            ActiveScene = JsonSerializer.Deserialize<Scene>(json);

            WatchCollection(ActiveScene.Entities);
        }
    }

    private void WatchCollection(ObservableCollection<GameEntity> collection)
    {
        collection.CollectionChanged += (s, e) =>
        {
            _isDirty = true;

            if (e.NewItems != null)
            {
                foreach (GameEntity entity in e.NewItems)
                {
                    WatchCollection(entity.Children);
                }
            }
        };

        foreach (var entity in collection)
        {
            WatchCollection(entity.Children);
        }
    }

    private void CheckAndSave()
    {
        if (_isDirty)
        {
            SaveScene();
            _isDirty = false;
        }
    }
}