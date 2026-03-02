using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using Component = WisdomWarrior.Engine.Core.Component;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class InspectorViewModel : ObservableObject
{
    private readonly SelectionManager _selectionManager;

    [ObservableProperty] private EntityTracker? _selectedEntity;

    public ObservableCollection<Component> Components => []; //SelectedEntity.TrackedComponents ?? new();
    public IEnumerable<string> AvailableComponentNames => ComponentRegistry.GetRegisteredKeys();

    public InspectorViewModel(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;

        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? obj)
    {
        if (obj == null)
        {
            SelectedEntity = null;
        }

        if (obj is EntityTracker entityTracker)
        {
            SelectedEntity = entityTracker;
        }
    }
}