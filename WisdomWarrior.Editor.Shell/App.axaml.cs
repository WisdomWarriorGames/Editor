using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;
using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Shell.Services;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();

        // 1. Create Layout
        var factory = new ShellLayoutFactory();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);

        // 2. Register
        serviceCollection.AddSingleton<IFactory>(factory);
        serviceCollection.AddSingleton<IDock>(layout);
        serviceCollection.AddSingleton<IShell, ShellService>();
        serviceCollection.AddSingleton<MainWindowViewModel>();

        var loader = new ModuleLoader();
        var pluginPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Plugins");
        loader.LoadModules(pluginPath, serviceCollection);

        Services = serviceCollection.BuildServiceProvider();

        loader.InitializeModules(Services);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}