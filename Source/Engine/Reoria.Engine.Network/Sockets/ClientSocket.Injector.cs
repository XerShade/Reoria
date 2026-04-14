using Autofac;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Engine.Network.Sockets;

/// <summary>
/// Defines an injector for registering the ClientSocket service in the application.
/// </summary>
/// <remarks>
/// This injector registers the ClientSocket as a singleton service for client-side applications.
/// It's excluded from server platforms since client sockets are only needed on client applications.
/// </remarks>
public class ClientSocketInjector : IApplicationServicesInjector
{
    /// <inheritdoc />
    public string Name
        => "ClientSocket Injector";

    /// <inheritdoc />
    public string Description
        => "Injects ClientSocket as a singleton service for client-side applications.";

    /// <inheritdoc />
    public Type[] Dependencies
        => [];

    /// <inheritdoc />
    public Platform Platform
        => Platform.All & ~Platform.Server;

    /// <inheritdoc />
    /// <remarks>
    /// Registers the ClientSocket as a singleton service to ensure only one instance
    /// exists per application lifetime, which is appropriate for network connections.
    /// </remarks>
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ClientSocket>().As<ClientSocket>().As<Socket>().SingleInstance();

    /// <inheritdoc />
    /// <remarks>
    /// No post-configuration setup is required for the ClientSocket service.
    /// </remarks>
    public void OnConfigureServices(IServiceProvider provider)
    { }
}