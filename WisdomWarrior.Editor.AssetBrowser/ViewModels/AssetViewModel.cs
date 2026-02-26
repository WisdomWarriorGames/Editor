using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetViewModel : ObservableObject
{
    private readonly FileSystemService _fileSystemService;

    [ObservableProperty] private bool _isSelected = false;

    [ObservableProperty] private string _name;
    [ObservableProperty] private string _extension;
    [ObservableProperty] private bool _isFolder;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;
    [ObservableProperty] private bool _isValid = true;

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

    public AssetViewModel(string directory, string name, FileSystemService fileSystemService)
    {
        TempName = name;
        IsFolder = true;
        IsEditing = true;
        FullPath = directory;
        _fileSystemService = fileSystemService;
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