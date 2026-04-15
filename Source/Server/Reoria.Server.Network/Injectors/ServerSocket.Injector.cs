using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Network.Sockets;
using Reoria.Server.Network.Sockets;

namespace Reoria.Server.Network.Injectors;

/// <summary>
/// Application services injector that registers server-side networking functionality.
/// This injector sets up the dependency injection container with the ServerSocket service
/// required for handling client connections and network communication on the server.
/// </summary>
public class ServerSocketInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector.
    /// </summary>
    public string Name 
        => "Server Socket Service Injector";

    /// <summary>
    /// Gets a description of what this injector does.
    /// </summary>
    public string Description 
        => "Registers ServerSocket for handling client connections and server-side network communication.";

    /// <summary>
    /// Gets the dependencies required by this injector.
    /// This injector has no external dependencies as all required services are injected through constructor parameters.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this injector should run.
    /// Server socket functionality is only required on the server platform.
    /// </summary>
    public Platform Platform
        => Platform.Server;

    /// <summary>
    /// Registers the ServerSocket service with the dependency injection container.
    /// Registers ServerSocket as a singleton service with both concrete and base interface registrations
    /// to ensure proper dependency resolution and maintain a single server socket instance.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ServerSocket>().As<ServerSocket>().As<Socket>().SingleInstance();

    /// <summary>
    /// Configures the registered services after the container is built.
    /// This injector does not require any additional configuration for the ServerSocket service.
    /// </summary>
    /// <param name="provider">The service provider containing the registered services.</param>
    public void OnConfigureServices(IServiceProvider provider)
    { 
        // No additional configuration required for ServerSocket
    }
}