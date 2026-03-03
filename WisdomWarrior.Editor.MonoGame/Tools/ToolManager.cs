namespace WisdomWarrior.Editor.MonoGame.Tools;

public class ToolManager(ToolContext context)
{
    private TranslateTool _translateTool = new();

    public void Update()
    {
        _translateTool.Update(context);
    }
}