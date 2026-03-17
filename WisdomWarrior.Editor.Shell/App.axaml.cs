using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Shell.ViewModels;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Shell;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        Configurations.Configurations.Configure(services);

        ServiceProvider = services.BuildServiceProvider();

        ComponentRegistry.Bootstrap();
        SystemRegistry.Bootstrap();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
