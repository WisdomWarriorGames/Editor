using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Threading;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.SceneList.Services;

public class SceneService
{
    private bool _isDirty;
    private readonly DispatcherTimer _saveTimer;

    private readonly WorkspaceService _workspaceService;

    public Scene ActiveScene { get; set; }

    public SceneService(WorkspaceService workspaceService)
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new ComponentConverter());

        var json = JsonSerializer.Serialize(ActiveScene, options);
        File.WriteAllText(_workspaceService.ActiveScene, json);
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        if (_workspaceService.ActiveScene != null)
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };
            options.Converters.Add(new ComponentConverter());

            var json = File.ReadAllText(_workspaceService.ActiveScene);
            ActiveScene = JsonSerializer.Deserialize<Scene>(json, options);
            ActiveScene.Initialize();

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
                    WatchComponents(entity.Components);
                }
            }
        };

        foreach (var entity in collection)
        {
            WatchCollection(entity.Children);
            WatchComponents(entity.Components);
        }
    }

    private void WatchComponents(ObservableCollection<Component> components)
    {
        components.CollectionChanged += (s, e) =>
        {
            _isDirty = true; // Component added/removed
            if (e.NewItems != null)
            {
                foreach (Component c in e.NewItems)
                    c.OnComponentChanged += () => _isDirty = true;
            }
        };

        foreach (var component in components)
        {
            component.OnComponentChanged += () => _isDirty = true;
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