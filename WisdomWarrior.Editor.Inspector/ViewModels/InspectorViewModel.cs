using System.Windows.Input;
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

    public bool CanAddItems => AddItemCommand != null;
    public IEnumerable<string> AvailableAddNames => CurrentContent switch
    {
        EntityInspectorViewModel entity => entity.AvailableComponentNames,
        SceneInspectorViewModel scene => scene.AvailableSystemNames,
        _ => Enumerable.Empty<string>()
    };

    public ICommand? AddItemCommand => CurrentContent switch
    {
        EntityInspectorViewModel entity => entity.AddComponentCommand,
        SceneInspectorViewModel scene => scene.AddSystemCommand,
        _ => null
    };

    public InspectorViewModel(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
        _selectionManager.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? obj)
    {
        if (CurrentContent is System.ComponentModel.INotifyPropertyChanged oldContent)
        {
            oldContent.PropertyChanged -= OnCurrentContentPropertyChanged;
        }

        if (obj == null)
        {
            CurrentContent = null;
            Name = "No Selection";
            NotifySelectionPropertiesChanged();
            return;
        }

        (CurrentContent, Name) = obj switch
        {
            EntityTracker entity => ((ObservableObject?)new EntityInspectorViewModel(entity), entity.Name),
            SceneTracker scene => ((ObservableObject?)new SceneInspectorViewModel(scene), scene.ActiveScene?.Name ?? "Scene"),
            FileSystemNode node => ((ObservableObject?)null, node.FileNameWithExtension),
            _ => ((ObservableObject?)null, "No Selection")
        };

        if (CurrentContent is System.ComponentModel.INotifyPropertyChanged newContent)
        {
            newContent.PropertyChanged += OnCurrentContentPropertyChanged;
        }

        NotifySelectionPropertiesChanged();
    }

    private void NotifySelectionPropertiesChanged()
    {
        OnPropertyChanged(nameof(CanAddItems));
        OnPropertyChanged(nameof(AvailableAddNames));
        OnPropertyChanged(nameof(AddItemCommand));
    }

    private void OnCurrentContentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EntityInspectorViewModel.AvailableComponentNames)
            or nameof(SceneInspectorViewModel.AvailableSystemNames))
        {
            OnPropertyChanged(nameof(AvailableAddNames));
        }
    }
}
