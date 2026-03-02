using System.ComponentModel;
using Avalonia.Input;
using ReactiveUI;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.MonoGame.ViewModels;

public class MonoGameViewModel : ReactiveObject
{
    private readonly SelectionManager _selectionManager;
    public EditorRuntime CurrentGame { get; set; } = new();

    public MonoGameViewModel(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.OnSelectionChanged += OnOnSelectionChanged;
    }

    private void OnOnSelectionChanged(object? obj)
    {
        if (obj is EntityTracker tracker)
        {
            CurrentGame.SelectedEntity = tracker.EngineEntity;
        }
    }
}