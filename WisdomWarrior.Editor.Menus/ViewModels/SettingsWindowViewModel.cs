using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Menus.Services;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public partial class SettingsWindowViewModel : ObservableObject
{
    public IReadOnlyList<SettingsCategoryViewModel> Categories { get; }

    [ObservableProperty]
    private SettingsCategoryViewModel _selectedCategory;

    public SettingsWindowViewModel(EditorThemeService editorThemeService)
    {
        var generalSettings = new GeneralSettingsViewModel(editorThemeService);
        Categories = new[] { generalSettings };
        _selectedCategory = generalSettings;
    }
}
