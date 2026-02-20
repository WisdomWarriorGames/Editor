using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using WisdomWarrior.Editor.Inspector.Models;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Attributes;

namespace WisdomWarrior.Editor.Inspector.Views;

public partial class ComponentEditor : UserControl
{
    public ComponentEditor()
    {
        InitializeComponent();
    }

    // This is called when the ContentControl binds a component to this view
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not Component component) return;

        PropertiesStack.Children.Clear();

        // Get all public properties (Position, Scale, Name, etc.)
        var properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            bool shouldHide = prop.GetCustomAttribute<HideInInspectorAttribute>() != null;

            if (shouldHide) continue;

            var row = CreatePropertyRow(prop, component);
            PropertiesStack.Children.Add(row);
        }

        component.OnComponentChanged += () =>
        {
            // Jump to the UI thread to update the view
            Dispatcher.UIThread.Post(RefreshAllProperties);
        };
    }

    private void RefreshAllProperties()
    {
        foreach (var panel in PropertiesStack.Children.OfType<StackPanel>())
        {
            var editor = panel.Children.OfType<ContentControl>().FirstOrDefault();
            if (editor?.Tag is Action refresh) refresh.Invoke();
        }
    }

    private Control CreatePropertyRow(PropertyInfo prop, Component target)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock { Text = prop.Name, Opacity = 0.5, FontSize = 10 });

        // Pick the editor based on Type
        Control editor = prop.PropertyType switch
        {
            Type t when t == typeof(Vector2) => CreateVector2Editor(prop, target),
            // Type t when t == typeof(float) => CreateFloatEditor(prop, target),
            // Type t when t == typeof(string) => CreateStringEditor(prop, target),
            _ => new TextBlock { Text = "Unsupported Type", FontStyle = FontStyle.Italic }
        };

        panel.Children.Add(editor);
        return panel;
    }

    private Control CreateVector2Editor(PropertyInfo prop, object target)
    {
        var currentVector = (Vector2)prop.GetValue(target)!;

        // Create the translator
        var vectorVM = new Vector2ViewModel(currentVector, (newVector) =>
        {
            prop.SetValue(target, newVector);

            if (target is ITrackableComponent trackable)
            {
                trackable.NotifyChanged();
            }
        });

        var editor = new ContentControl
        {
            DataContext = vectorVM,
            Content = vectorVM,
            ContentTemplate = this.FindResource("Vector2Template") as DataTemplate
        };

        editor.Tag = new Action(() => { vectorVM.UpdateFromEngine((Vector2)prop.GetValue(target)!); });

        return editor;
    }
}