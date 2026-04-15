using Autofac;
using Reoria.Client.Network.Sockets;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Network.Sockets;

namespace Reoria.Client.Network.Injectors;

/// <summary>
/// Application services injector that registers client-side networking functionality.
/// This injector sets up the dependency injection container with the ClientSocket service
/// required for connecting to servers and handling client-side network communication.
/// </summary>
/// <remarks>
/// This injector is specifically designed for client platforms and is excluded from server builds
/// since client socket functionality is only required on client applications. The ClientSocket
/// is registered as a singleton to maintain a single network connection instance throughout
/// the application lifetime, which is essential for stable client-server communication.
/// </remarks>
public class ClientSocketInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector.
    /// </summary>
    public string Name 
        => "Client Socket Service Injector";

    /// <summary>
    /// Gets a description of what this injector does.
    /// </summary>
    public string Description 
        => "Registers ClientSocket for connecting to servers and handling client-side network communication.";

    /// <summary>
    /// Gets the dependencies required by this injector.
    /// This injector has no external dependencies as all required services are injected through constructor parameters.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this injector should run.
    /// Client socket functionality is required on all client platforms (Desktop, Windows, iOS, Android) but excluded from server.
    /// </summary>
    public Platform Platform
        => Platform.All & ~Platform.Server;

    /// <summary>
    /// Registers the ClientSocket service with the dependency injection container.
    /// Registers ClientSocket as a singleton service with both concrete and base interface registrations
    /// to ensure proper dependency resolution and maintain a single client socket instance for stable networking.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ClientSocket>().As<ClientSocket>().As<Socket>().SingleInstance();

    /// <summary>
    /// Configures the registered services after the container is built.
    /// This injector does not require any additional configuration for the ClientSocket service.
    /// </summary>
    /// <param name="provider">The service provider containing the registered services.</param>
    public void OnConfigureServices(IServiceProvider provider)
    { 
        // No additional configuration required for ClientSocket
    }
}