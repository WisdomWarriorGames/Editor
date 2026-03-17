using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Menus.Services;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public partial class GeneralSettingsViewModel : SettingsCategoryViewModel
{
    private readonly EditorThemeService _editorThemeService;

    public IReadOnlyList<EditorTheme> AvailableThemes { get; } = new[]
    {
        EditorTheme.Dark,
        EditorTheme.Light
    };

    [ObservableProperty]
    private EditorTheme _selectedTheme;

    public GeneralSettingsViewModel(EditorThemeService editorThemeService) : base("General")
    {
        _editorThemeService = editorThemeService;
        _editorThemeService.EnsureInitialized();
        _selectedTheme = _editorThemeService.CurrentTheme;
    }

    partial void OnSelectedThemeChanged(EditorTheme value)
    {
        _editorThemeService.SetTheme(value);
    }
}
