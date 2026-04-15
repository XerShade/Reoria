using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Server.Network.Sessions;
using Reoria.Server.Network.Sessions.Interfaces;

namespace Reoria.Server.Network.Injectors;

/// <summary>
/// Application services injector that registers network session management functionality.
/// This injector sets up the dependency injection container with session management services
/// required for handling client connections and session lifecycle on the server.
/// </summary>
public class SessionsInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector.
    /// </summary>
    public string Name 
        => "Network Session Functionality Injector";

    /// <summary>
    /// Gets a description of what this injector does.
    /// </summary>
    public string Description 
        => "Adds functionality for creating and managing network sessions.";

    /// <summary>
    /// Gets the dependencies required by this injector.
    /// This injector has no external dependencies.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this injector should run.
    /// Session management is only required on the server platform.
    /// </summary>
    public Platform Platform
        => Platform.Server;

    /// <summary>
    /// Registers the session management services with the dependency injection container.
    /// Registers SessionManager as the implementation of ISessionManager with per-dependency lifetime.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<SessionManager>().As<ISessionManager>().InstancePerDependency();

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