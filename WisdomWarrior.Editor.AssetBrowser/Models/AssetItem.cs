using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WisdomWarrior.Editor.AssetBrowser.Models;

public partial class AssetItem : ObservableObject
{
    private readonly Action<AssetItem, string>? _onCommit;
    private readonly Action<AssetItem>? _onCancel;
    private readonly Action<AssetItem, KeyModifiers>? _onSelected;
    private readonly Action<AssetItem>? _onDelete;
    private readonly Action<AssetItem>? _onDoubleClicked;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private Bitmap? _thumbnail;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isSelected = false;

    [ObservableProperty]
    private bool _isValid = true;

    private string _originalName = string.Empty;

    public string FullPath { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public string Extension { get; set; } = string.Empty;

    public string FileNameWithExtension => IsFolder ? Name : $"{Name}{Extension}";

    public bool IsNew { get; set; }

    public string Icon => IsFolder ? "📁" : GetFileIcon(Extension);

    public bool IsImage => Extension.ToLower() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => true,
        _ => false
    };

    public AssetItem(
        Action<AssetItem, string>? onCommit = null,
        Action<AssetItem>? onCancel = null,
        Action<AssetItem, KeyModifiers>? onSelected = null,
        Action<AssetItem>? onDelete = null,
        Action<AssetItem>? onDoubleClicked = null)
    {
        _onCommit = onCommit;
        _onCancel = onCancel;
        _onSelected = onSelected;
        _onDelete = onDelete;
        _onDoubleClicked = onDoubleClicked;
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
        if (!IsEditing || !IsValid) return;

        IsEditing = false;
        _onCommit?.Invoke(this, FileNameWithExtension);
    }

    [RelayCommand]
    private void CancelRename()
    {
        if (!IsEditing) return;

        Name = _originalName;
        IsEditing = false;
        _onCommit?.Invoke(this, FileNameWithExtension);
    }

    [RelayCommand]
    private void Select(KeyModifiers modifiers)
    {
        _onSelected?.Invoke(this, modifiers);
    }

    [RelayCommand]
    private void Delete()
    {
        _onDelete?.Invoke(this);
    }

    [RelayCommand]
    public void DoubleClick()
    {
        _onDoubleClicked?.Invoke(this);
    }

    private string GetFileIcon(string ext) => ext switch
    {
        ".png" or ".jpg" => "🖼️",
        ".cs" => "📝",
        ".audio" => "🎵",
        _ => "📄"
    };
}