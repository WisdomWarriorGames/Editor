using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.Xna.Framework;
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
        UpdateEditorCursor(viewModel);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var viewModel = DataContext as MonoGameViewModel;
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