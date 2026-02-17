using ReactiveUI;

namespace WisdomWarrior.Editor.MonoGame.ViewModels;

public class MonoGameViewModel : ReactiveObject
{
    public EditorRuntime CurrentGame { get; set; } = new();
}