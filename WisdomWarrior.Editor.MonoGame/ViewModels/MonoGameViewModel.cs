using System.Numerics;
using ReactiveUI;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.MonoGame.Tools;

namespace WisdomWarrior.Editor.MonoGame.ViewModels;

public class MonoGameViewModel : ReactiveObject
{
    private readonly SelectionManager _selectionManager;
    private readonly CurrentSceneManager _currentSceneManager;
    private readonly ToolContext _toolContext;

    public EditorInputService InputService { get; }
    public EditorRuntime CurrentGame { get; set; }

    public MonoGameViewModel(
        SelectionManager selectionManager,
        CurrentSceneManager currentSceneManager,
        EditorInputService inputService)
    {
        InputService = inputService;
        _toolContext = new ToolContext(InputService);
        CurrentGame = new EditorRuntime(_toolContext);

        _selectionManager = selectionManager;
        _currentSceneManager = currentSceneManager;
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    public void TrySelectEntityAtViewportPoint(Vector2 viewportPoint)
    {
        var selectedEntity = CurrentGame.HitTestEntityAtViewportPoint(viewportPoint);
        if (selectedEntity == null)
        {
            return;
        }

        var entityTracker = _currentSceneManager.Tracker.FindTrackerByEntity(selectedEntity);
        if (entityTracker == null)
        {
            return;
        }

        _selectionManager.SetSelection(entityTracker);
    }

    private void OnSelectionChanged(object? obj)
    {
        if (obj is EntityTracker entityTracker)
        {
            _toolContext.SetSelectedEntity(entityTracker.EngineEntity);
            return;
        }

        _toolContext.SetSelectedEntity(null);
    }
}