using Microsoft.Extensions.DependencyInjection;

namespace WisdomWarrior.Editor.Core;

public interface IEditorModule
{
    /// <summary>
    /// Called when the Editor is starting up.
    /// Use this to register your Services (logic) and Views (UI).
    /// </summary>
    /// <param name="services">The DI container to add your logic to.</param>
    void RegisterTypes(IServiceCollection services);

    /// <summary>
    /// Called after all services are registered.
    /// Use this to open your default windows.
    /// </summary>
    /// <param name="shell">The main window interface.</param>
    void Initialize(IServiceProvider provider);
}