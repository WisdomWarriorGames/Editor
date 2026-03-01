using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace WisdomWarrior.Editor.Core.DragAndDrop;

public class DropTarget : AvaloniaObject
{
    public static readonly AttachedProperty<ICommand?> DropCommandProperty =
        AvaloniaProperty.RegisterAttached<DropTarget, Control, ICommand?>("DropCommand");

    public static void SetDropCommand(AvaloniaObject element, ICommand? value) =>
        element.SetValue(DropCommandProperty, value);

    public static ICommand? GetDropCommand(AvaloniaObject element) =>
        element.GetValue(DropCommandProperty);

    static DropTarget()
    {
        DropCommandProperty.Changed.AddClassHandler<Control>(OnDropCommandChanged);
    }

    private static void OnDropCommandChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue != null)
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            control.AddHandler(DragDrop.DropEvent, OnDrop);
        }
        else
        {
            DragDrop.SetAllowDrop(control, false);
            control.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
            control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            control.RemoveHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        if (sender is not Control control) return;
        var command = GetDropCommand(control);
        if (command == null) return;

        var isAccepted = false;

        if (e.Data.Contains("EntityData"))
        {
            var draggedItem = e.Data.Get("EntityData");

            if (draggedItem is System.Collections.IEnumerable list && draggedItem is not string)
            {
                foreach (var item in list)
                {
                    if (command.CanExecute(item))
                    {
                        isAccepted = true;
                        break;
                    }
                }
            }
            else if (command.CanExecute(draggedItem))
            {
                isAccepted = true;
            }

            if (isAccepted) e.DragEffects = DragDropEffects.Move;
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null)
            {
                // UNPACK OS FILES
                foreach (var file in files)
                {
                    if (command.CanExecute(file))
                    {
                        isAccepted = true;
                        break;
                    }
                }
            }

            if (isAccepted) e.DragEffects = DragDropEffects.Copy;
        }

        if (isAccepted)
        {
            e.Handled = true;
            ((IPseudoClasses)control.Classes).Add(":dragover");
        }
        else
        {
            ((IPseudoClasses)control.Classes).Remove(":dragover");
        }
    }

    private static void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (sender is Control control)
        {
            ((IPseudoClasses)control.Classes).Remove(":dragover");
        }
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        if (sender is not Control control) return;
        ((IPseudoClasses)control.Classes).Remove(":dragover");

        var command = GetDropCommand(control);
        if (command == null) return;

        bool handledDrop = false;

        if (e.Data.Contains("EntityData"))
        {
            var payload = e.Data.Get("EntityData");
            if (command.CanExecute(payload))
            {
                command.Execute(payload);
                handledDrop = true;
            }
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && command.CanExecute(files))
            {
                command.Execute(files);
                handledDrop = true;
            }
        }

        if (handledDrop) e.Handled = true;
    }
}