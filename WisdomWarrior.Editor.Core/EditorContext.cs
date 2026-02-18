using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core;

public partial class EditorContext : ObservableObject
{
    [ObservableProperty]
    private GameEntity? _selectedEntity;
}