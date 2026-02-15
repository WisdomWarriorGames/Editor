using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Models;
using WisdomWarrior.Editor.AssetBrowser.Services;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetBrowserViewModel : ObservableObject
{
    private readonly ContentService _contentService;

    [ObservableProperty]
    private string _currentPath;

    private AssetItem? _lastSelectedItem;

    public ObservableCollection<AssetItem> Items { get; } = new();

    public AssetBrowserViewModel(string projectPath)
    {
        _contentService = new ContentService(projectPath);
        _currentPath = _contentService.RootPath;
        _contentService.RefreshRequested += OnRefreshRequested;
        Refresh();
    }

    private void OnRefreshRequested()
    {
        if (Items.Any(i => i.IsEditing)) return;

        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    public void Refresh()
    {
        Items.Clear();
        var rawItems = _contentService.GetItems(CurrentPath);

        foreach (var item in rawItems)
        {
            var viewModelItem = new AssetItem(OnCommitRename, OnCancelRename, OnItemSelected)
            {
                Name = item.Name,
                FullPath = item.FullPath,
                IsFolder = item.IsFolder,
                Extension = item.Extension,
                IsNew = false
            };
            Items.Add(viewModelItem);
        }
    }

    [RelayCommand]
    public void CreateFolder()
    {
        var baseName = "New Folder";
        var finalName = baseName;
        var count = 1;

        while (Items.Any(x => x.Name == finalName))
        {
            finalName = $"{baseName} ({count++})";
        }

        var newItem = new AssetItem(OnCommitRename, OnCancelRename, OnItemSelected)
        {
            Name = finalName,
            FullPath = Path.Combine(CurrentPath, finalName),
            IsFolder = true,
            IsNew = true
        };

        Items.Add(newItem);
        newItem.BeginEdit();
    }

    private void OnCancelRename(AssetItem item)
    {
        if (item.IsNew)
        {
            Items.Remove(item);
        }
    }

    private void OnCommitRename(AssetItem item, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;

        try
        {
            if (item.IsNew)
            {
                _contentService.CreateFolder(CurrentPath, newName);
                item.IsNew = false;
            }
            else
            {
                var currentName = Path.GetFileName(item.FullPath);
                if (currentName == newName) return;

                _contentService.RenameItem(item.FullPath, newName, item.IsFolder);
            }

            item.FullPath = Path.Combine(CurrentPath, newName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Operation failed: {ex.Message}");
        }
        finally
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(Refresh);
        }
    }

    private void OnItemSelected(AssetItem item, KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            item.IsSelected = !item.IsSelected;

            if (item.IsSelected) _lastSelectedItem = item;
        }
        else if (modifiers.HasFlag(KeyModifiers.Shift) && _lastSelectedItem != null)
        {
            var start = Items.IndexOf(_lastSelectedItem);
            var end = Items.IndexOf(item);

            if (start != -1 && end != -1)
            {
                // Clear others first? usually Shift+Click keeps previous selection in Windows Explorer
                // but let's stick to simple range for now.
                // ClearSelection(); // Optional: Uncomment if you want Shift to clear previous non-range selections

                var lower = Math.Min(start, end);
                var upper = Math.Max(start, end);

                for (int i = lower; i <= upper; i++)
                {
                    Items[i].IsSelected = true;
                }
            }
        }
        else
        {
            ClearSelection();
            item.IsSelected = true;
            _lastSelectedItem = item;
        }
    }

    [RelayCommand]
    public void ClearSelection()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].IsSelected = false;
            Items[i].CancelRenameCommand.Execute(null);
        }

        _lastSelectedItem = null;
    }
}