using System.Text.Json;
using System.Text.Json.Serialization;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Converters;

namespace WisdomWarrior.Editor.Core.Services;

public class CurrentSceneManager
{
    private string _activeScenePath = string.Empty;
    private bool _isDirty;
    private readonly System.Timers.Timer _saveTimer;

    private int _ticksPerSecond = 20;
    private bool _isTicking;

    public int TicksPerSecond
    {
        get => _ticksPerSecond;
        set => _ticksPerSecond = Math.Max(1, value);
    }

    public Scene? ActiveScene { get; private set; }
    public SceneTracker Tracker { get; } = new();

    public event Action? CurrentSceneReady;

    public CurrentSceneManager()
    {
        Tracker.OnSceneModified += () => _isDirty = true;

        _saveTimer = new System.Timers.Timer(1000)
        {
            AutoReset = true
        };
        _saveTimer.Elapsed += (sender, args) => CheckAndSave();
    }

    public void LoadScene(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        _activeScenePath = path;

        var options = new JsonSerializerOptions { IncludeFields = true };
        options.Converters.Add(new ComponentConverter());
        options.Converters.Add(new SystemDrawingColorJsonConverter());

        var json = File.ReadAllText(path);
        ActiveScene = JsonSerializer.Deserialize<Scene>(json, options);
        if (ActiveScene == null) return;

        NormalizeSceneAssetPathsForStorage();
        ActiveScene.Initialize();

        SceneManager.AddScene(ActiveScene.Name, ActiveScene);
        SceneManager.SetCurrentScene(ActiveScene.Name);

        Tracker.TrackScene(ActiveScene);

        _saveTimer.Start();
        CurrentSceneReady?.Invoke();

        StartTicking();
    }

    public void StartTicking()
    {
        if (_isTicking) return;
        _isTicking = true;

        _ = Task.Run(EditorTickLoop);
    }

    private async Task EditorTickLoop()
    {
        while (_isTicking)
        {
            Tracker.Update();

            var delayMs = 1000 / TicksPerSecond;
            await Task.Delay(delayMs);
        }
    }

    public bool SaveScene()
    {
        if (string.IsNullOrWhiteSpace(_activeScenePath) || ActiveScene == null) return false;

        NormalizeSceneAssetPathsForStorage();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new ComponentConverter());
        options.Converters.Add(new SystemDrawingColorJsonConverter());

        try
        {
            var json = JsonSerializer.Serialize(ActiveScene, options);
            File.WriteAllText(_activeScenePath, json);

            Tracker.AcknowledgeSaved();
            _isDirty = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CheckAndSave()
    {
        if (_isDirty)
        {
            SaveScene();
        }
    }

    public void StopTicking()
    {
        _isTicking = false;
        _saveTimer.Stop();
    }

    private void NormalizeSceneAssetPathsForStorage()
    {
        if (ActiveScene == null) return;

        foreach (var rootEntity in ActiveScene.Entities)
        {
            NormalizeEntityAssetsRecursive(rootEntity);
        }
    }

    private static void NormalizeEntityAssetsRecursive(GameEntity entity)
    {
        foreach (var sprite in entity.Components.OfType<Sprite>())
        {
            if (sprite.Image == null) continue;

            sprite.Image.AssetPath = AssetHelpers.NormalizeAssetPathForStorage(sprite.Image.AssetPath);
        }

        foreach (var child in entity.Children)
        {
            NormalizeEntityAssetsRecursive(child);
        }
    }
}
