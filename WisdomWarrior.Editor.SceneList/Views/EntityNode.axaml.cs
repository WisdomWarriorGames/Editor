using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace WisdomWarrior.Editor.SceneList.Views;

public partial class EntityNode : UserControl
{
    public EntityNode()
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
}