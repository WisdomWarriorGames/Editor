using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class InspectorViewModel : ObservableObject
{
    private readonly SelectionManager _selectionManager;
    [ObservableProperty] private ObservableObject? _currentContent;
    [ObservableProperty] private string? _name = "No Selection";

    public InspectorViewModel(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? obj)
    {
        if (obj == null)
        {
            CurrentContent = null;
            Name = "No Selection";
            return;
        }

        (CurrentContent, Name) = obj switch
        {
            EntityTracker entity => (new EntityInspectorViewModel(entity), entity.Name),
            FileSystemNode node => (null, node.FileNameWithExtension),

            _ => (null, "No Selection")
        };
    }
}