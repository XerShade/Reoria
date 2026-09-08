using Autofac;
using Reoria.Client.Network.Sockets;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Network.Sockets;

namespace Reoria.Client.Network.Injectors;

/// <summary>
/// Bootstrap services phase participant that registers client-side networking functionality.
/// This phase participant sets up the dependency injection container with the ClientSocket service
/// required for connecting to servers and handling client-side network communication.
/// </summary>
/// <remarks>
/// This phase participant is specifically designed for client platforms and is excluded from server builds
/// since client socket functionality is only required on client applications. The ClientSocket
/// is registered as a singleton to maintain a single network connection instance throughout
/// the application lifetime, which is essential for stable client-server communication.
/// 
/// Runs during bootstrap phase - no constructor dependencies allowed.
/// </remarks>
public class ClientSocketInjector : IBootstrapServices
{
    /// <summary>
    /// Gets the name of this bootstrap phase participant.
    /// </summary>
    public string Name
        => "Client Socket Service Bootstrap";

    /// <summary>
    /// Gets a description of what this bootstrap phase participant does.
    /// </summary>
    public string Description
        => "Registers ClientSocket for connecting to servers and handling client-side network communication during bootstrap.";

    /// <summary>
    /// Gets the dependencies required by this bootstrap phase participant.
    /// This phase participant has no external dependencies as all required services are injected through method parameters.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this bootstrap phase participant should run.
    /// Client socket functionality is required on all client platforms (Desktop, Windows, iOS, Android) but excluded from server.
    /// </summary>
    public Platform Platform
        => Platform.All & ~Platform.Server;

    /// <summary>
    /// Registers the ClientSocket service with the dependency injection container during bootstrap.
    /// Registers ClientSocket as a singleton service with both concrete and base interface registrations
    /// to ensure proper dependency resolution and maintain a single client socket instance for stable networking.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnRegisterServices(ContainerBuilder services)
        => services.RegisterType<ClientSocket>().As<ClientSocket>().As<Socket>().SingleInstance();
}