using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.AssetBrowser.Views;

public partial class AssetBrowserViewOld : UserControl
{
    public AssetBrowserViewOld()
    {
        InitializeComponent();
    }

    private void OnBackgroundClicked(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is AssetBrowserViewModelOld vm)
        {
            vm.ClearSelection();
        }
    }
}