using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Helpers;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetViewModel : ObservableObject, IDroppableAsset, IAssetDropData
{
    public FileSystemNode? Node { get; }

    private readonly FileSystemService _fileSystemService;
    private readonly IAssetClipboardActionService _clipboardActionService;
    private readonly SelectionManager? _selectionManager;
    private readonly Action<AssetViewModel>? _cancelEdit;

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _extension = string.Empty;
    [ObservableProperty] private bool _isFolder;
    [ObservableProperty] private string _tempName = string.Empty;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isValid = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasThumbnail))]
    private Bitmap? _thumbnail;

    public bool HasThumbnail => Thumbnail != null;

    public string DisplayName => Name;

    public bool IsNew { get; set; }

    public string FullPath { get; private set; }

    public string Icon => IsFolder ? "📁" : "📄";

    public bool IsImage => AssetHelpers.IsImage(Extension.ToLowerInvariant());

    public bool IsAudio => Extension.ToLowerInvariant() switch
    {
        ".wav" or ".mp3" or ".ogg" or ".flac" or ".m4a" or ".wma" => true,
        _ => false
    };

    public AssetViewModel(
        FileSystemNode node,
        FileSystemService fileSystemService,
        IAssetClipboardActionService clipboardActionService,
        SelectionManager selectionManager)
    {
        Node = node;
        _fileSystemService = fileSystemService;
        _clipboardActionService = clipboardActionService;
        _selectionManager = selectionManager;

        Name = node.Name;
        Extension = node.Extension;
        IsFolder = node.IsFolder;
        FullPath = node.FullPath;

        InitializeThumbnail();
    }

    public AssetViewModel(
        string directory,
        string name,
        FileSystemService fileSystemService,
        IAssetClipboardActionService clipboardActionService,
        Action<AssetViewModel> cancelEdit)
    {
        TempName = name;
        IsFolder = true;
        IsEditing = true;
        FullPath = directory;
        _fileSystemService = fileSystemService;
        _clipboardActionService = clipboardActionService;
        _cancelEdit = cancelEdit;
        IsNew = true;

        ValidateName(name);
        InitializeThumbnail();
    }

    public Task ProcessAsync(string destinationDirectory, IProgress<string>? progress = null)
    {
        _fileSystemService.Move(destinationDirectory, FullPath, progress);
        return Task.CompletedTask;
    }

    public async Task<Bitmap?> LoadThumbnailAsync(string path, int width = 128)
    {
        var attempts = 0;
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

    private void ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsValid = false;
            return;
        }

        var directory = Path.GetDirectoryName(FullPath);
        if (directory == null)
        {
            return;
        }

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
        return IsFolder && droppedItem.CanAccept(this);
    }

    private bool CanPasteInto()
    {
        return IsFolder;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private void AcceptDrop(object? droppedItem)
    {
        droppedItem.ProcessFileSystemDropAsync(FullPath, _fileSystemService);
    }

    [RelayCommand]
    private async Task Copy()
    {
        await _clipboardActionService.CopyPathsAsync([FullPath]);
    }

    [RelayCommand(CanExecute = nameof(CanPasteInto))]
    private async Task PasteInto()
    {
        await _clipboardActionService.PasteIntoAsync(FullPath);
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
        if (string.IsNullOrEmpty(TempName) || !IsValid)
        {
            return;
        }

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
        var path = IsFolder ? FullPath : Path.GetDirectoryName(FullPath);
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    [RelayCommand]
    public void ViewProperties()
    {
        if (_selectionManager == null || Node == null)
        {
            return;
        }

        _selectionManager.SetSelection(Node);
    }
}
