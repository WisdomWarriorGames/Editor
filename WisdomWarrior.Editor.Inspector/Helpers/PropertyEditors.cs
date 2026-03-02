using System.Numerics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Models;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Inspector.Helpers;

public static class PropertyEditors
{
    public static Control CreateVector2Editor(this UserControl control, PropertyTracker prop)
    {
        var currentVector = (Vector2)prop.GetValue()!;
        
        var vectorVM = new Vector2ViewModel(currentVector, (newVector) =>
        {
            prop.SetValue(newVector);
        });

        var editor = new ContentControl
        {
            DataContext = vectorVM,
            Content = vectorVM,
            ContentTemplate = control.FindResource("Vector2Template") as DataTemplate
        };

        editor.Tag = new Action(() => { vectorVM.UpdateFromEngine((Vector2)prop.GetValue()!); });

        return editor;
    }

    public static Control CreateFloatEditor(this UserControl control, PropertyTracker prop)
    {
        var currentFloat = (float)prop.GetValue()!;

        // Create the translator
        var floatVM = new FloatViewModel(currentFloat, (newFloat) =>
        {
            prop.SetValue(newFloat);
        });

        var editor = new ContentControl
        {
            DataContext = floatVM,
            Content = floatVM,
            ContentTemplate = control.FindResource("FloatTemplate") as DataTemplate
        };

        editor.Tag = new Action(() => { floatVM.UpdateFromEngine((float)prop.GetValue()!); });

        return editor;
    }
}