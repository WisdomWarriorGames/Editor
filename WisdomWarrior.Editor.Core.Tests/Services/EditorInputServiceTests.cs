using WisdomWarrior.Editor.Core.Services;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class EditorInputServiceTests
{
    [Fact]
    public void SetLeftMouseDown_TransitionToDown_MarksPressedThisFrame()
    {
        var input = new EditorInputService();

        input.SetLeftMouseDown(true);

        Assert.True(input.IsLeftMouseDown);
        Assert.True(input.LeftPressedThisFrame);
        Assert.False(input.LeftReleasedThisFrame);
    }

    [Fact]
    public void SetLeftMouseDown_TransitionToUp_MarksReleasedThisFrame()
    {
        var input = new EditorInputService();
        input.SetLeftMouseDown(true);
        input.AdvanceFrame();

        input.SetLeftMouseDown(false);

        Assert.False(input.IsLeftMouseDown);
        Assert.False(input.LeftPressedThisFrame);
        Assert.True(input.LeftReleasedThisFrame);
    }

    [Fact]
    public void AdvanceFrame_ClearsTransientMouseFlags()
    {
        var input = new EditorInputService();
        input.SetLeftMouseDown(true);
        input.SetLeftMouseDown(false);

        input.AdvanceFrame();

        Assert.False(input.LeftPressedThisFrame);
        Assert.False(input.LeftReleasedThisFrame);
    }
}
