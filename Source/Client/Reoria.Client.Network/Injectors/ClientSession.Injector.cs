using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Client.Network.Sessions;
using Reoria.Client.Network.Sessions.Interfaces;

namespace Reoria.Client.Network.Injectors;

/// <summary>
/// Application services injector that registers client-side session management functionality.
/// This injector sets up the dependency injection container with session management services
/// required for handling client-side session data and entity ownership tracking.
/// </summary>
public class ClientSessionInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector.
    /// </summary>
    public string Name 
        => "Client Session Management Injector";

    /// <summary>
    /// Gets a description of what this injector does.
    /// </summary>
    public string Description 
        => "Adds functionality for managing client-side session data and entity ownership.";

    /// <summary>
    /// Gets the dependencies required by this injector.
    /// This injector has no external dependencies.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this injector should run.
    /// Client session management is only required on client platforms.
    /// </summary>
    public Platform Platform
        => Platform.Desktop;

    /// <summary>
    /// Registers the client session management services with the dependency injection container.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnBuildServices(ContainerBuilder services)
    {
        // Register client session management services
        services.RegisterType<ClientSessionManager>().As<IClientSessionManager>().SingleInstance();
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
