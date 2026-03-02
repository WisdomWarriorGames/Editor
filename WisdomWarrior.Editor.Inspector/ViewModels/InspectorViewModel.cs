using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class InspectorViewModel : ObservableObject
{
    private readonly SelectionManager _selectionManager;

    [ObservableProperty] private EntityTracker? _selectedEntity;

    public IEnumerable<ComponentTracker> Components => SelectedEntity?.TrackedComponents ?? Enumerable.Empty<ComponentTracker>();

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

        OnPropertyChanged(nameof(Components));
    }
}