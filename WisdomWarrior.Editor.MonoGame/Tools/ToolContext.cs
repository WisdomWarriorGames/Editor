using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.MonoGame.Tools;

public class ToolContext(EditorInputService inputService)
{
    public EditorInputService Input { get; } = inputService;
    public GameEntity? SelectedEntity { get; private set; }

    public void SetSelectedEntity(GameEntity? entity)
    {
        SelectedEntity = entity;
    }
}