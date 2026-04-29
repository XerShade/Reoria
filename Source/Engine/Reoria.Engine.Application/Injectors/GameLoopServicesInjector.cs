using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.GameLoop.Extensions;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// An injector that registers game loop services in the DI container.
/// </summary>
public class GameLoopServicesInjector : IApplicationServicesInjector
{
    /// <inheritdoc />
    public string Name => "Game Loop Services Injector";

    /// <inheritdoc />
    public string Description => "Registers game loop phase registry and factory services.";

    /// <inheritdoc />
    public Type[] Dependencies => [];

    /// <inheritdoc />
    public Platform Platform => Platform.All;

    /// <inheritdoc />
    public void OnBuildServices(ContainerBuilder services) =>
        // Register all game loop services and built-in phases
        services.AddGameLoop();

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    {
        // No additional configuration needed
    }
}