using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;


namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class EditorDocument : Document
{
    [ObservableProperty]
    private object? _contentView;
}