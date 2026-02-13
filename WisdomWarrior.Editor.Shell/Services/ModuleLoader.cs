using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WisdomWarrior.Editor.Core;

namespace WisdomWarrior.Editor.Shell.Services;

public class ModuleLoader
{
    // We keep a list of loaded modules so we can "Unload" them later if needed
    private readonly List<IEditorModule> _loadedModules = new();

    public void LoadModules(string pluginDirectory, IServiceCollection services)
    {
        // 1. Ensure the folder exists
        if (!Directory.Exists(pluginDirectory))
        {
            Directory.CreateDirectory(pluginDirectory);
            return;
        }

        // 2. Find all DLLs
        var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll");

        foreach (var dllPath in dllFiles)
        {
            try
            {
                // 3. Load the Assembly (The DLL file)
                byte[] assemblyData = File.ReadAllBytes(dllPath);
                var assembly = Assembly.Load(assemblyData);

                // 4. Scan for the "IEditorModule" interface
                var moduleType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IEditorModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (moduleType != null)
                {
                    // 5. Create an instance of the module
                    var module = (IEditorModule)Activator.CreateInstance(moduleType)!;

                    // 6. Let the module register its own services (Dependency Injection)
                    module.RegisterTypes(services);

                    _loadedModules.Add(module);

                    System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Loaded: {moduleType.Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Failed to load {dllPath}: {ex.Message}");
            }
        }
    }

    // Call this AFTER the ServiceProvider is built
    public void InitializeModules(IServiceProvider provider)
    {
        foreach (var module in _loadedModules)
        {
            module.Initialize(provider);
        }
    }
}