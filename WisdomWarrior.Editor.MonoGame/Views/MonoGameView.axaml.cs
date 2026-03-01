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
        var point = e.GetPosition(this);

        var viewModel = DataContext as MonoGameViewModel;
        viewModel.CurrentGame.LocalMousePosition = new Vector2((float)point.X, (float)point.Y);
        viewModel.CurrentGame.CursorScale = (float)(VisualRoot?.RenderScaling ?? 1.0f);
        // viewModel.CurrentGame.UpdateObjectPosition();
        UpdateEditorCursor(viewModel);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MonoGameViewModel;
        if (viewModel == null) return;

        // 1. Grab the exact position right as the click happens
        var point = e.GetPosition(this);
        viewModel.CurrentGame.LocalMousePosition = new Vector2((float)point.X, (float)point.Y);

        // 2. Start dragging
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            viewModel.CurrentGame.StartDragging();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var viewModel = DataContext as MonoGameViewModel;
        viewModel.CurrentGame.StopDragging();
    }

    private void UpdateEditorCursor(MonoGameViewModel viewModel)
    {
        if (viewModel.CurrentGame.IsHovering)
        {
            this.Cursor = new Cursor(StandardCursorType.Hand);
        }
        else
        {
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }
}