using Avalonia;
using Avalonia.Styling;
using SukiUI;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Menus.Services;

public class EditorThemeService(EditorSettingsService editorSettingsService)
{
    private EditorSettings _settings = new();
    private bool _isInitialized;

    public EditorTheme CurrentTheme
    {
        get
        {
            EnsureInitialized();
            return _settings.EditorTheme;
        }
    }

    public void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _settings = editorSettingsService.Load();
        ApplyTheme(_settings.EditorTheme);
        _isInitialized = true;
    }

    public void SetTheme(EditorTheme theme)
    {
        EnsureInitialized();

        if (_settings.EditorTheme != theme)
        {
            _settings.EditorTheme = theme;
            editorSettingsService.Save(_settings);
        }

        ApplyTheme(theme);
    }

    private static void ApplyTheme(EditorTheme theme)
    {
        var themeVariant = theme == EditorTheme.Light
            ? ThemeVariant.Light
            : ThemeVariant.Dark;

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = themeVariant;
        }

        SukiTheme.GetInstance().ChangeBaseTheme(themeVariant);
    }
}
