using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Network.Sockets;
using Reoria.Server.Network.Sockets;

namespace Reoria.Server.Network.Injectors;

/// <summary>
/// Bootstrap services phase participant that registers server-side networking functionality.
/// This phase participant sets up the dependency injection container with the ServerSocket service
/// required for handling client connections and network communication on the server.
/// </summary>
/// <remarks>
/// Runs during bootstrap phase - no constructor dependencies allowed.
/// </remarks>
public class ServerSocketBootstrap : IBootstrapServices
{
    /// <summary>
    /// Gets the name of this bootstrap phase participant.
    /// </summary>
    public string Name
        => "Server Socket Service Bootstrap";

    /// <summary>
    /// Gets a description of what this bootstrap phase participant does.
    /// </summary>
    public string Description
        => "Registers ServerSocket for handling client connections and server-side network communication during bootstrap.";

    /// <summary>
    /// Gets the dependencies required by this bootstrap phase participant.
    /// This phase participant has no external dependencies as all required services are injected through method parameters.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform on which this bootstrap phase participant should run.
    /// Server socket functionality is only required on the server platform.
    /// </summary>
    public Platform Platform
        => Platform.Server;

    /// <summary>
    /// Registers the ServerSocket service with the dependency injection container during bootstrap.
    /// Registers ServerSocket as a singleton service with both concrete and base interface registrations
    /// to ensure proper dependency resolution and maintain a single server socket instance.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    public void OnRegisterServices(ContainerBuilder services)
        => services.RegisterType<ServerSocket>().As<ServerSocket>().As<Socket>().SingleInstance();
}