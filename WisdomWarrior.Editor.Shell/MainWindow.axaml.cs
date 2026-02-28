using Avalonia.Controls;
using Avalonia.Styling;
using SukiUI;
using SukiUI.Controls;

namespace WisdomWarrior.Editor.Shell;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();

        SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Dark);
    }
}