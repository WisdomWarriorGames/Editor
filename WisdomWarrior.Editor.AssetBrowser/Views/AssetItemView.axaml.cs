using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using WisdomWarrior.Editor.AssetBrowser.Models;

namespace WisdomWarrior.Editor.AssetBrowser.Views;

public partial class AssetItemView : UserControl
{
    public AssetItemView()
    {
        InitializeComponent();

        var nameBox = this.FindControl<TextBox>("NameTextBox");

        if (nameBox != null)
        {
            nameBox.PropertyChanged += (sender, e) =>
            {
                if (e.Property == IsVisibleProperty && e.NewValue is true)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        nameBox.Focus();
                        nameBox.SelectAll();
                    });
                }
            };
        }
    }

    public void OnTextBoxLoaded(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Focus();
            tb.SelectAll();
        }
    }

    private void OnCardClicked(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
    
        if (properties.IsLeftButtonPressed ||  properties.IsRightButtonPressed)
        {
            // 1. Capture the modifiers and item while we are on the event thread
            var modifiers = e.KeyModifiers;
            var item = DataContext as AssetItem;

            if (item != null)
            {
                // 2. Use the Dispatcher to ensure the command runs on the UI Thread
                Dispatcher.UIThread.Post(() =>
                {
                    if (sender is Control c)
                    {
                        c.Focus();
                    }
                    
                    if (item.SelectCommand.CanExecute(modifiers))
                    {
                        item.SelectCommand.Execute(modifiers);
                    }
                });
            
                e.Handled = true;
            }
        }
    }
}