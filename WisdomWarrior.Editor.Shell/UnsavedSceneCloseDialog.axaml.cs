using Avalonia.Controls;
using SukiUI.Controls;

namespace WisdomWarrior.Editor.Shell;

public partial class UnsavedSceneCloseDialog : SukiWindow
{
    public UnsavedSceneCloseDialog()
    {
        InitializeComponent();
    }

    private void OnSaveAndCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(UnsavedSceneCloseChoice.SaveAndClose);
    }

    private void OnDontSaveClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(UnsavedSceneCloseChoice.DontSave);
    }

    private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close(UnsavedSceneCloseChoice.Cancel);
    }
}
