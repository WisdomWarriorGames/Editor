using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace WisdomWarrior.Editor.Core.DragAndDrop;

public class DragSource : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<DragSource, Control, bool>("IsEnabled");

    public static void SetIsEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(AvaloniaObject element) =>
        element.GetValue(IsEnabledProperty);

    static DragSource()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is bool isEnabled)
        {
            if (isEnabled)
            {
                control.PointerMoved += OnPointerMoved;
            }
            else
            {
                control.PointerMoved -= OnPointerMoved;
            }
        }
    }

    private static async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is Control control && e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            control.PointerMoved -= OnPointerMoved;

            var data = new DataObject();
            if (control.DataContext != null)
            {
                data.Set("EntityData", control.DataContext);

                await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            }

            control.PointerMoved += OnPointerMoved;
        }
    }
}