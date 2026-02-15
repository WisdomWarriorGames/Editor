using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Media.Imaging;
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
            string displayName = item.IsFolder 
                ? item.Name 
                : Path.GetFileNameWithoutExtension(item.Name);
            
            var viewModelItem = new AssetItem(OnCommitRename, OnCancelRename, OnItemSelected, OnDeleteSelected, OnItemDoubleClicked)
            {
                Name = displayName,
                FullPath = item.FullPath,
                IsFolder = item.IsFolder,
                Extension = item.Extension,
                IsNew = false
            };
            Items.Add(viewModelItem);
            
            if (viewModelItem.IsImage)
            {
                LoadThumbnailAsync(viewModelItem);
            }
        }
    }
    
    private async void LoadThumbnailAsync(AssetItem item)
    {
        try
        {
            await Task.Run(() =>
            {
                using var stream = File.OpenRead(item.FullPath);
                var bitmap = Bitmap.DecodeToWidth(stream, 100);
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() => item.Thumbnail = bitmap);
            });
        }
        catch
        {
        }
    }

    private void OnDeleteSelected(AssetItem item)
    {
        if (!item.IsNew)
        {
            _contentService.DeleteItem(item.FullPath, item.IsFolder);
        }

        Items.Remove(item);
    }

    private void OnCancelRename(AssetItem item)
    {
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
    
    private void OnItemDoubleClicked(AssetItem item)
    {
        if (item.IsFolder)
        {
            CurrentPath = item.FullPath;
            Refresh();
        }
        else
        {
            // Future: Open file in editor
            Console.WriteLine($"Opening file: {item.Name}");
        }
    }

    [RelayCommand]
    public void NavigateUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent != null && parent.FullName.StartsWith(_contentService.RootPath))
        {
            CurrentPath = parent.FullName;
            Refresh();
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

        var newItem = new AssetItem(OnCommitRename, OnCancelRename, OnItemSelected, OnDeleteSelected, OnItemDoubleClicked)
        {
            Name = finalName,
            FullPath = Path.Combine(CurrentPath, finalName),
            IsFolder = true,
            IsNew = true
        };

        Items.Add(newItem);
        newItem.BeginEdit();
    }

    [RelayCommand]
    public void DeleteSelected()
    {
        var selectedItems = Items.Where(x => x.IsSelected).ToList();

        if (selectedItems.Count == 0) return;

        foreach (var item in selectedItems)
        {
            try
            {
                if (!item.IsNew)
                {
                    _contentService.DeleteItem(item.FullPath, item.IsFolder);
                }

                Items.Remove(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete {item.Name}: {ex.Message}");
            }
        }

        Refresh();
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