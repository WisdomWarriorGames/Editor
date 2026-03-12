using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using WisdomWarrior.Editor.MonoGame.ViewModels;

namespace WisdomWarrior.Editor.MonoGame.Views;

public partial class MonoGameView : UserControl
{
    private static readonly Cursor HiddenCursor = new(StandardCursorType.None);
    private static readonly Cursor ArrowCursor = new(StandardCursorType.Arrow);

    public MonoGameView()
    {
        InitializeComponent();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (DataContext is not MonoGameViewModel viewModel) return;

        var input = viewModel.InputService;
        input.MousePosition = GetPointerPosition(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (DataContext is not MonoGameViewModel viewModel) return;
        var input = viewModel.InputService;
        var pointerPosition = GetPointerPosition(e);
        input.MousePosition = pointerPosition;

        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed)
        {
            input.SetLeftMouseDown(true);
            viewModel.TrySelectEntityAtViewportPoint(pointerPosition);
            Cursor = HiddenCursor;
        }

        if (props.IsRightButtonPressed)
        {
            input.IsRightMouseDown = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (DataContext is not MonoGameViewModel viewModel) return;
        var input = viewModel.InputService;
        var pointerPosition = GetPointerPosition(e);
        input.MousePosition = pointerPosition;

        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed)
        {
            input.SetLeftMouseDown(false);
            Cursor = ArrowCursor;
        }

        if (!props.IsRightButtonPressed)
        {
            input.IsRightMouseDown = false;
        }
    }

    private Vector2 GetPointerPosition(PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        return new Vector2((float)point.X, (float)point.Y);
    }
}
