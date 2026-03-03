using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using WisdomWarrior.Editor.MonoGame.ViewModels;

namespace WisdomWarrior.Editor.MonoGame.Views;

public partial class MonoGameView : UserControl
{
    public MonoGameView()
    {
        InitializeComponent();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (DataContext is not MonoGameViewModel viewModel) return;

        var input = viewModel.InputService;

        var point = e.GetPosition(this);
        var newPosition = new Vector2((float)point.X, (float)point.Y);

        input.MousePosition = newPosition;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (DataContext is not MonoGameViewModel viewModel) return;
        var input = viewModel.InputService;

        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsLeftButtonPressed)
        {
            input.IsLeftMouseDown = true;
            Cursor = new Cursor(StandardCursorType.None);
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

        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed)
        {
            input.IsLeftMouseDown = false;
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        if (!props.IsRightButtonPressed)
        {
            input.IsRightMouseDown = false;
        }
    }
}