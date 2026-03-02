using System.Numerics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Helpers;
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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not ComponentTracker tracker) return;

        PropertiesStack.Children.Clear();

        foreach (var propTracker in tracker.Properties)
        {
            var shouldHide = propTracker.GetCustomAttribute<HideInInspectorAttribute>() != null;

            if (shouldHide) continue;

            var row = CreatePropertyRow(propTracker);
            PropertiesStack.Children.Add(row);
        }
    }

    private Control CreatePropertyRow(PropertyTracker prop)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock { Text = prop.Name, Opacity = 0.5, FontSize = 10 });

        var editor = prop.PropertyType switch
        {
            Type t when t == typeof(Vector2) => this.CreateVector2Editor(prop),
            Type t when t == typeof(float) => this.CreateFloatEditor(prop),
            // Type t when t == typeof(string) => CreateStringEditor(prop, target),
            _ => new TextBlock { Text = "Unsupported Type", FontStyle = FontStyle.Italic }
        };

        panel.Children.Add(editor);
        return panel;
    }
}