using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.AssetBrowser.Views;

public partial class AssetBrowserView : UserControl
{
    public AssetBrowserView()
    {
        InitializeComponent();
    }

    private void OnBackgroundClicked(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is AssetBrowserViewModel vm)
        {
            vm.ClearSelection();
        }
    }
}