using ReactiveUI;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.MonoGame.Tools;

namespace WisdomWarrior.Editor.MonoGame.ViewModels;

public class MonoGameViewModel : ReactiveObject
{
    private readonly SelectionManager _selectionManager;
    private readonly ToolContext _toolContext;

    public EditorInputService InputService { get; }
    public EditorRuntime CurrentGame { get; set; }

    public MonoGameViewModel(SelectionManager selectionManager, EditorInputService inputService)
    {
        InputService = inputService;
        _toolContext = new ToolContext(InputService);
        CurrentGame = new EditorRuntime(_toolContext);

        _selectionManager = selectionManager;
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? obj)
    {
        if (obj == null)
        {
            _toolContext.SetSelectedEntity(null);
            return;
        }

        if (obj is EntityTracker entityTracker)
        {
            _toolContext.SetSelectedEntity(entityTracker.EngineEntity);
        }
    }
}