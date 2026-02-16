using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell;

public partial class App : Application
{
    private const string TEST_PROJECT_PATH = @"C:\Personal\GameDev\TestGame\TestGame";
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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var registry = ServiceProvider.GetRequiredService<FileSystemRegistry>();
            registry.Initialize(TEST_PROJECT_PATH);

            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}