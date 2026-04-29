using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Server.Network.Players;
using Reoria.Server.Network.Players.Interfaces;
using Reoria.Server.Network.Security;
using Reoria.Server.Network.Security.Interfaces;

namespace Reoria.Server.Network.Injectors;

/// <summary>
/// Application services injector that registers player management and security functionality.
/// This injector sets up the dependency injection container with services required for
/// server authoritative multiplayer operations including player management and security validation.
/// </summary>
public class PlayersInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector.
    /// </summary>
    public string Name
        => "Player Management and Security Injector";

    /// <summary>
    /// Gets a description of what this injector does.
    /// </summary>
    public string Description
        => "Adds functionality for managing players, permissions, and security validation.";

    /// <summary>
    /// Gets the dependencies required by this injector.
    /// This injector depends on the session management functionality.
    /// </summary>
    public Type[] Dependencies
        => [typeof(SessionsInjector)];

    /// <summary>
    /// Gets the platform on which this injector should run.
    /// Player management and security are only required on the server platform.
    /// </summary>
    public Platform Platform
        => Platform.Server;

    /// <summary>
    /// Registers the player management and security services with the dependency injection container.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnBuildServices(ContainerBuilder services)
    {
        // Register player management services
        _ = services.RegisterType<PlayerManager>().As<IPlayerManager>().SingleInstance();

        // Register security services
        _ = services.RegisterType<SecurityValidator>().As<ISecurityValidator>().InstancePerDependency();
        _ = services.RegisterType<EntityOwnershipService>().As<IEntityOwnershipService>().SingleInstance();
    }

    /// <summary>
    /// Configures the registered services after the container is built.
    /// This injector does not require any additional configuration.
    /// </summary>
    /// <param name="provider">The service provider containing the registered services.</param>
    public void OnConfigureServices(IServiceProvider provider)
    {
        // Does not require any configuration.
    }
}