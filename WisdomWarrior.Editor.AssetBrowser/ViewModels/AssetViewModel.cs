using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.AssetBrowser.Helpers;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasThumbnail))]
    private Bitmap? _thumbnail;

    public bool HasThumbnail => Thumbnail != null;

    public string DisplayName => Name;

    public bool IsNew = false;
    public string FullPath;

    public string Icon => IsFolder ? "📁" : "📄";

    public bool IsImage => Extension.ToLower() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => true,
        _ => false
    };

    public bool IsAudio => Extension.ToLower() switch
    {
        ".wav" or ".mp3" or ".ogg" or ".flac" or ".m4a" or ".wma" => true,
        _ => false
    };

    public AssetViewModel(FileSystemNode node, FileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;

        Name = node.Name;
        Extension = node.Extension;
        IsFolder = node.IsFolder;
        FullPath = node.FullPath;

        InitializeThumbnail();
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

    private void InitializeThumbnail()
    {
        Task.Run(async () =>
        {
            if (IsFolder)
            {
                await SetBitmap(ThumbnailHelper.FOLDER_ICON);
            }
            else if (IsImage)
            {
                await SetBitmap(FullPath);
            }
            else if (IsAudio)
            {
                await SetBitmap(ThumbnailHelper.SOUND_ICON);
            }
            else
            {
                await SetBitmap(ThumbnailHelper.FILE_ICON);
            }
        });
    }

    private async Task SetBitmap(string path)
    {
        var bitmap = await LoadThumbnailAsync(path, 32);
        if (bitmap != null)
        {
            Dispatcher.UIThread.Invoke(() => Thumbnail = bitmap);
        }
    }

    public async Task<Bitmap?> LoadThumbnailAsync(string path, int width = 128)
    {
        int attempts = 0;
        while (attempts < 5)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return Bitmap.DecodeToWidth(stream, width);
                });
            }
            catch (IOException)
            {
                attempts++;
                await Task.Delay(100);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
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