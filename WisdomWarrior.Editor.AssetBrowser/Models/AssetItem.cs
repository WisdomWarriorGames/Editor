using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WisdomWarrior.Editor.AssetBrowser.Models;

public partial class AssetItem : ObservableObject
{
    private readonly Action<AssetItem, string>? _onCommit;
    private readonly Action<AssetItem>? _onCancel;
    private readonly Action<AssetItem, KeyModifiers>? _onSelected;

    [ObservableProperty]
    private string _name = string.Empty;

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

    public bool IsNew { get; set; }

    public string Icon => IsFolder ? "📁" : GetFileIcon(Extension);

    public AssetItem(
        Action<AssetItem, string>? onCommit = null,
        Action<AssetItem>? onCancel = null,
        Action<AssetItem, KeyModifiers>? onSelected = null)
    {
        _onCommit = onCommit;
        _onCancel = onCancel;
        _onSelected = onSelected;
    }

    partial void OnNameChanged(string value)
    {
        ValidateName(value);
    }

    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            IsValid = false;
            return;
        }

        var directory = Path.GetDirectoryName(FullPath);
        if (directory == null) return;

        var newPath = Path.Combine(directory, name);

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
        _onCommit?.Invoke(this, Name);
    }

    [RelayCommand]
    private void CancelRename()
    {
        if (!IsEditing) return;

        Name = _originalName;
        IsEditing = false;
        _onCancel?.Invoke(this);
    }

    [RelayCommand]
    private void Select(KeyModifiers modifiers)
    {
        _onSelected?.Invoke(this, modifiers);
    }

    private string GetFileIcon(string ext) => ext switch
    {
        ".png" or ".jpg" => "🖼️",
        ".cs" => "📝",
        ".audio" => "🎵",
        _ => "📄"
    };
}