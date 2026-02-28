using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetItem : ObservableObject
{
    private readonly IAssetHandler _handler;
    private string _originalName = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isValid = true;
    [ObservableProperty] private Bitmap? _thumbnail;
    
    public string Icon => IsFolder ? "📁" : GetFileIcon(Extension);

    public bool IsImage => Extension.ToLower() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => true,
        _ => false
    };

    public string FullPath { get; set; }
    public bool IsFolder { get; set; }
    public string Extension { get; set; }
    public bool IsNew { get; set; }

    public AssetItem(IAssetHandler handler, FileSystemNode node)
    {
        _handler = handler;

        FullPath = node.FullPath;
        IsFolder = node.IsFolder;
        Extension = node.Extension;
        Name = node.Name;
    }
    
    partial void OnNameChanged(string value)
    {
        ValidateName(value);
    }

    private void ValidateName(string nameOnly)
    {
        if (string.IsNullOrWhiteSpace(nameOnly)) { IsValid = false; return; }

        var directory = Path.GetDirectoryName(FullPath);
        if (directory == null) return;

        // Reconstruct the full name for the check
        var fullName = IsFolder ? nameOnly : $"{nameOnly}{Extension}";
        var newPath = Path.Combine(directory, fullName);
    
        var exists = Directory.Exists(newPath) || File.Exists(newPath);
        var isDifferentFile = !string.Equals(FullPath, newPath, StringComparison.OrdinalIgnoreCase);

        IsValid = !(exists && isDifferentFile);
    }

    [RelayCommand]
    public void BeginEdit()
    {
        _originalName = Name;
        IsEditing = true;
    }

    [RelayCommand]
    private void CommitRename()
    {
        if (!IsValid) return;
        IsEditing = false;
        _handler.OnRenameCommitted(this, IsFolder ? Name : $"{Name}{Extension}");
    }

    [RelayCommand]
    private void CancelRename()
    {
        Name = _originalName;
        IsEditing = false;
    }

    [RelayCommand] private void Select(KeyModifiers mods) => _handler.OnSelected(this, mods);
    [RelayCommand] private void DoubleClick() => _handler.OnDoubleClicked(this);
    [RelayCommand] private void Delete() => _handler.OnDeleteRequested(this);
    
    private string GetFileIcon(string ext) => ext switch
    {
        ".png" or ".jpg" => "🖼️",
        ".cs" => "📝",
        ".audio" => "🎵",
        _ => "📄"
    };
}