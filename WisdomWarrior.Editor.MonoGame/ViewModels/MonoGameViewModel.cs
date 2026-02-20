using System.ComponentModel;
using Avalonia.Input;
using ReactiveUI;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.MonoGame.ViewModels;

public class MonoGameViewModel : ReactiveObject
{
    private readonly EditorContext _context;
    public EditorRuntime CurrentGame { get; set; } = new();

    public MonoGameViewModel(EditorContext context)
    {
        _context = context;

        _context.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorContext.SelectedEntity))
        {
            CurrentGame.SelectedEntity = _context.SelectedEntity;
        }
    }
}