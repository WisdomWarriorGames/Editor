using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using Component = WisdomWarrior.Engine.Core.Component;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class InspectorViewModel : ObservableObject
{
    private readonly EditorContext _context;

    public GameEntity? SelectedEntity => _context.SelectedEntity;
    public ObservableCollection<Component> Components => SelectedEntity?.Components ?? new();

    public InspectorViewModel(EditorContext context)
    {
        _context = context;

        _context.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorContext.SelectedEntity))
        {
            OnPropertyChanged(nameof(SelectedEntity));
            OnPropertyChanged(nameof(Components));
        }
    }

    [RelayCommand]
    private void AddComponent(string typeName)
    {
        if (SelectedEntity == null) return;

        if (typeName == "Transform")
        {
            if (!SelectedEntity.Components.Any(c => c is Transform))
            {
                SelectedEntity.AddComponent<Transform>();
            }
        }

        OnPropertyChanged(nameof(Components));
    }
}