using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Shell;

public class ViewLocator : IDataTemplate
{
    // 1. "Match" checks if this Locator handles the data
    public bool Match(object? data)
    {
        return data is ObservableObject; // Only handle our ViewModels
    }

    // 2. "Build" creates the UI
    public Control? Build(object? data)
    {
        if (data is null) return null;

        // 1. Convert ViewModel name to View name
        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    
        // 2. SEARCH ALL LOADED DLLs (Assemblies)
        // This allows the Shell to "see" inside the AssetBrowser.dll
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(name))
            .FirstOrDefault(t => t != null);

        if (type != null)
        {
            // 3. Create the View and return it
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }
}