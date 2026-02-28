using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace WisdomWarrior.Editor.Core.DragAndDrop;

public class DragSource : AvaloniaObject
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<DragSource, Control, bool>("IsEnabled");

    public static void SetIsEnabled(AvaloniaObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(AvaloniaObject element) =>
        element.GetValue(IsEnabledProperty);
    
    private static Point _dragStartPoint;
    private const double DragThreshold = 3.0;

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
                control.PointerPressed += OnPointerPressed;
                control.PointerMoved += OnPointerMoved;
            }
            else
            {
                control.PointerPressed -= OnPointerPressed;
                control.PointerMoved -= OnPointerMoved;
            }
        }
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            _dragStartPoint = e.GetPosition(null);
            
            var listBox = FindAncestorOfType<ListBox>(control);
            if (listBox?.SelectedItems != null && control.DataContext != null)
            {
                var selectedItems = listBox.SelectedItems.Cast<object>().ToList();

                if (selectedItems.Count > 1 && selectedItems.Contains(control.DataContext))
                {
                    if ((e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Shift)) == 0)
                    {
                        e.Handled = true;
                    }
                }
            }
        }
    }

    private static async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is Control control && e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            // 2. CALCULATE DISTANCE
            var currentPoint = e.GetPosition(null);
            var diff = _dragStartPoint - currentPoint;
            var distance = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);

            // 3. ONLY START DRAG IF WE MOVED PAST THE THRESHOLD
            if (distance < DragThreshold)
            {
                return; 
            }

            var data = new DataObject();
            if (control.DataContext != null)
            {
                object payload = control.DataContext;
                var listBox = FindAncestorOfType<ListBox>(control);

                if (listBox != null && listBox.SelectedItems != null)
                {
                    var selectedItems = listBox.SelectedItems.Cast<object>().ToList();

                    if (selectedItems.Contains(control.DataContext))
                    {
                        payload = selectedItems;
                    }
                }

                data.Set("EntityData", payload);
                await DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            }
        }
    }

    private static T? FindAncestorOfType<T>(Visual? visual) where T : Visual
    {
        while (visual != null)
        {
            if (visual is T ancestor) return ancestor;
            visual = visual.GetVisualParent();
        }

        return null;
    }
}