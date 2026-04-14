using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.GameLoop.Extensions;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Client.Core.Application;

/// <summary>
/// An injector that registers game loop services for the client application.
/// </summary>
public class ClientGameLoopInjector : IApplicationServicesInjector
{
    /// <inheritdoc />
    public string Name => "Client Game Loop Injector";

    /// <inheritdoc />
    public string Description => "Registers game loop services for the client application.";

    /// <inheritdoc />
    public Type[] Dependencies => [];

    /// <inheritdoc />
    public Platform Platform => Platform.Desktop | Platform.Windows | Platform.iOS | Platform.Android;

    /// <inheritdoc />
    public void OnBuildServices(ContainerBuilder services)
    {
        // Register game loop services only (phases are registered elsewhere)
        services.AddGameLoopServices();
    }

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    {
        // No additional configuration needed
    }
}
