using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Menus.ViewModels;

public abstract class SettingsCategoryViewModel(string title) : ObservableObject
{
    public string Title { get; } = title;
}
