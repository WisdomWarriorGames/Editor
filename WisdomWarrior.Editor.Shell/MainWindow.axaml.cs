using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using SukiUI.Controls;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell;

public partial class MainWindow : SukiWindow
{
    private bool _allowClose;
    private bool _isHandlingCloseRequest;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose)
            return;

        if (DataContext is not MainWindowViewModel viewModel)
            return;

        if (!viewModel.HasUnsavedInMemoryDirtyScene)
        {
            if (viewModel.HasDirtyPersistedScene && !viewModel.TryAutoSavePersistedScene())
            {
                e.Cancel = true;
                EditorUI.ToastManager.CreateToast()
                    .WithTitle("Scene auto-save failed. Please save manually before closing.")
                    .Queue();
            }

            return;
        }

        e.Cancel = true;

        if (_isHandlingCloseRequest)
            return;

        _isHandlingCloseRequest = true;
        _ = HandleUnsavedSceneCloseRequestAsync(viewModel);
    }

    private async Task HandleUnsavedSceneCloseRequestAsync(MainWindowViewModel viewModel)
    {
        try
        {
            var decision = await new UnsavedSceneCloseDialog().ShowDialog<UnsavedSceneCloseChoice>(this);

            if (decision == UnsavedSceneCloseChoice.Cancel)
                return;

            if (decision == UnsavedSceneCloseChoice.DontSave)
            {
                _allowClose = true;
                Close();
                return;
            }

            var selectedDirectory = await PromptForSceneSaveDirectory();
            if (string.IsNullOrWhiteSpace(selectedDirectory))
                return;

            if (!viewModel.TrySaveSceneToDirectory(selectedDirectory))
            {
                EditorUI.ToastManager.CreateToast()
                    .WithTitle("Scene could not be saved to the selected folder.")
                    .Queue();
                return;
            }

            _allowClose = true;
            Close();
        }
        finally
        {
            _isHandlingCloseRequest = false;
        }
    }

    private async Task<string?> PromptForSceneSaveDirectory()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Save Scene Inside Solution",
            AllowMultiple = false
        });

        if (folders.Count == 0)
            return null;

        return folders[0].Path.LocalPath;
    }
}
