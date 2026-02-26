using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.AssetBrowser.Views;

public partial class Directory : UserControl
{
    public Directory()
    {
        InitializeComponent();
    }

    private void OnBackgroundClicked(object? sender, PointerPressedEventArgs e)
    {
        var visual = e.Source as Visual;

        while (visual != null)
        {
            if (visual is ListBoxItem)
            {
                return;
            }

            visual = visual.GetVisualParent();
        }

        if (sender is ListBox listBox && listBox.SelectedItems != null)
        {
            listBox.SelectedItems.Clear();

            if (DataContext is DirectoryViewModel viewModel)
            {
                viewModel.ResetChanges();
            }
        }
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var visual = e.Source as Visual;
        if (visual.DataContext is AssetViewModel assetViewModel && DataContext is DirectoryViewModel viewModel)
        {
            viewModel.Navigate(assetViewModel);
        }
    }
}