using System.Diagnostics;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetViewModel : ObservableObject, IDroppableAsset
{
    private readonly FileSystemService _fileSystemService;
    private readonly Action<AssetViewModel> _cancelEdit;

    [ObservableProperty] private bool _isSelected = false;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _extension;
    [ObservableProperty] private bool _isFolder;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;
    [ObservableProperty] private bool _isValid = true;

    public string DisplayName => Name;

    public bool IsNew = false;
    public string FullPath;

    public string Icon => IsFolder ? "📁" : "📄";

    public AssetViewModel(FileSystemNode node, FileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;

        Name = node.Name;
        Extension = node.Extension;
        IsFolder = node.IsFolder;
        FullPath = node.FullPath;
    }

    public AssetViewModel(string directory, string name, FileSystemService fileSystemService, Action<AssetViewModel> cancelEdit)
    {
        TempName = name;
        IsFolder = true;
        IsEditing = true;
        FullPath = directory;
        _fileSystemService = fileSystemService;
        _cancelEdit = cancelEdit;
        IsNew = true;

        ValidateName(name);
    }

    partial void OnTempNameChanged(string value)
    {
        ValidateName(value);
    }

    private void ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsValid = false;
            return;
        }

        var directory = Path.GetDirectoryName(FullPath);
        if (directory == null) return;

        var fullName = IsFolder ? value : $"{value}{Extension}";
        var newPath = Path.Combine(directory, fullName);

        var exists = Directory.Exists(newPath) || File.Exists(newPath);

        if (IsNew && exists)
        {
            IsValid = false;
            return;
        }

        var isDifferentFile = !string.Equals(FullPath, newPath, StringComparison.OrdinalIgnoreCase);

        IsValid = !(exists && isDifferentFile);
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (!IsFolder) return false;

        if (droppedItem.CanAccept(this)) return true;

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private async Task AcceptDrop(object? droppedItem)
    {
        droppedItem.ProcessDropAsync(FullPath, _fileSystemService);
    }

    public async Task ProcessAsync(string destinationDirectory, IProgress<string>? progress = null)
    {
        _fileSystemService.Move(destinationDirectory, FullPath, progress);
    }

    [RelayCommand]
    public void BeginEdit()
    {
        IsEditing = true;
        TempName = Name;
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditing = false;
        TempName = string.Empty;

        if (IsNew)
        {
            _cancelEdit?.Invoke(this);
        }
    }

    [RelayCommand]
    public void CommitEdit()
    {
        if (string.IsNullOrEmpty(TempName)) return;
        if (!IsValid) return;
        if (string.Equals(TempName, Name, StringComparison.InvariantCultureIgnoreCase))
        {
            CancelEdit();
            return;
        }

        if (IsFolder)
        {
            _fileSystemService.RenameFolder(FullPath, TempName);
        }
        else
        {
            _fileSystemService.RenameFile(FullPath, TempName);
        }

        CancelEdit();
    }

    [RelayCommand]
    public void Delete()
    {
        if (IsFolder)
        {
            _fileSystemService.DeleteFolder(FullPath);
        }
        else
        {
            _fileSystemService.DeleteFile(FullPath);
        }
    }

    [RelayCommand]
    public void OpenInExplorer()
    {
        var path = FullPath;

        if (!IsFolder)
        {
            path = Path.GetDirectoryName(path);
        }

        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}