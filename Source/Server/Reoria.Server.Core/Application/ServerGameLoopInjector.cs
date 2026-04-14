using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.GameLoop;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Server.Core.Application;

/// <summary>
/// An injector that registers game loop services for the server application.
/// </summary>
public class ServerGameLoopInjector : IApplicationServicesInjector
{
    /// <inheritdoc />
    public string Name => "Server Game Loop Injector";

    /// <inheritdoc />
    public string Description => "Registers game loop services for the server application.";

    /// <inheritdoc />
    public Type[] Dependencies => [];

    /// <inheritdoc />
    public Platform Platform => Platform.Server;

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
