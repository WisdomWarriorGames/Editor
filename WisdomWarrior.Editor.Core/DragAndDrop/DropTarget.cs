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
        if (sender is Control control && e.Data.Contains("EntityData"))
        {
            var command = GetDropCommand(control);
            var draggedItem = e.Data.Get("EntityData");

            if (command != null && command.CanExecute(draggedItem))
            {
                e.DragEffects = DragDropEffects.Move;
                e.Handled = true;
                
                ((IPseudoClasses)control.Classes).Add(":dragover");
                return;
            }
        }
        
        e.DragEffects = DragDropEffects.None;
        e.Handled = true;
        
        if (sender is Control c) ((IPseudoClasses)c.Classes).Remove(":dragover");
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
        if (sender is Control control)
        {
            ((IPseudoClasses)control.Classes).Remove(":dragover");

            if (e.Data.Contains("EntityData"))
            {
                var command = GetDropCommand(control);
                var draggedItem = e.Data.Get("EntityData");

                if (command != null && command.CanExecute(draggedItem))
                {
                    command.Execute(draggedItem);
                    e.Handled = true;
                }
            }
        }
    }
}