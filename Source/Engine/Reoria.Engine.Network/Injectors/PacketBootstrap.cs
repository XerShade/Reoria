using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Core.Reflection;
using Reoria.Engine.Network.Packets;
using Reoria.Engine.Network.Packets.Interfaces;

namespace Reoria.Engine.Network.Injectors;

/// <summary>
/// Bootstrap services phase participant that automatically discovers and registers packet handlers and composers.
/// This phase participant scans all loaded assemblies for classes implementing IIncomingPacket or IOutgoingPacket
/// and registers them with the dependency injection container for automatic packet system functionality.
/// </summary>
/// <remarks>
/// Runs during bootstrap phase - no constructor dependencies allowed.
/// </remarks>
public class PacketBootstrap : IBootstrapServices
{
    /// <summary>
    /// Gets the name of this bootstrap phase participant for identification purposes.
    /// </summary>
    public string Name
        => "Packet System Bootstrap";

    /// <summary>
    /// Gets a description of what this bootstrap phase participant does and its purpose in the application.
    /// </summary>
    public string Description
        => "Adds packet functionality to the game by automatically discovering and registering packet handlers and composers during bootstrap.";

    /// <summary>
    /// Gets the array of dependencies required by this bootstrap phase participant.
    /// This phase participant has no external dependencies.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platforms on which this bootstrap phase participant should be active.
    /// This phase participant runs on all platforms (Client, Server, etc.).
    /// </summary>
    public Platform Platform
        => Platform.All;

    /// <summary>
    /// Configures the dependency injection container by registering packet-related services during bootstrap.
    /// This method is called during application startup to set up the packet system.
    /// </summary>
    /// <param name="services">The container builder used to register application services.</param>
    public void OnRegisterServices(ContainerBuilder services)
    {
        // Register the main packet manager as a singleton service.
        _ = services.RegisterType<PacketManager>()
            .As<IPacketManager>()
            .SingleInstance();

        // Automatically discover and register all packet handlers and composers.
        this.DiscoverIncomingPackets(services);
        this.DiscoverOutgoingPackets(services);
    }

    /// <summary>
    /// Discovers and registers all classes that implement IIncomingPacket from loaded assemblies.
    /// Uses reflection to scan all currently loaded assemblies for packet handler implementations.
    /// </summary>
    /// <param name="services">The container builder used to register the discovered packet handlers.</param>
    protected virtual void DiscoverIncomingPackets(ContainerBuilder services)
    {
        // Find all concrete types that implement IIncomingPacket (excluding interfaces and abstract classes).
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<IIncomingPacket>();

        // Register each discovered packet handler with the dependency injection container.
        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IIncomingPacket>()
                .InstancePerDependency();
        }
    }

    /// <summary>
    /// Discovers and registers all classes that implement IOutgoingPacket from loaded assemblies.
    /// Uses reflection to scan all currently loaded assemblies for packet composer implementations.
    /// </summary>
    /// <param name="services">The container builder used to register the discovered packet composers.</param>
    protected virtual void DiscoverOutgoingPackets(ContainerBuilder services)
    {
        // Find all concrete types that implement IOutgoingPacket (excluding interfaces and abstract classes).
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<IOutgoingPacket>();

        // Register each discovered packet composer with the dependency injection container.
        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IOutgoingPacket>()
                .InstancePerDependency();
        }
    }
}