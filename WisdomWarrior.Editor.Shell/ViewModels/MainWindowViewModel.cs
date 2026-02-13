using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private IRootDock _layout;

    public MainWindowViewModel(IDock layout)
    {
        Layout = (IRootDock)layout;
    }
}