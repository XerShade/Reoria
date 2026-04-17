using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Core.Reflection;
using Reoria.Engine.Network.Packets;
using Reoria.Engine.Network.Packets.Interfaces;

namespace Reoria.Engine.Network.Injectors;

/// <summary>
/// Application service injector that automatically discovers and registers packet handlers and composers.
/// This injector scans all loaded assemblies for classes implementing IIncomingPacket or IOutgoingPacket
/// and registers them with the dependency injection container for automatic packet system functionality.
/// </summary>
public class PacketInjector : IApplicationServicesInjector
{
    /// <summary>
    /// Gets the name of this injector for identification purposes.
    /// </summary>
    public string Name 
        => "Packet System";

    /// <summary>
    /// Gets a description of what this injector does and its purpose in the application.
    /// </summary>
    public string Description 
        => "Adds packet functionality to the game by automatically discovering and registering packet handlers and composers.";

    /// <summary>
    /// Gets the array of dependencies required by this injector.
    /// This injector has no external dependencies.
    /// </summary>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platforms on which this injector should be active.
    /// This injector runs on all platforms (Client, Server, etc.).
    /// </summary>
    public Platform Platform
        => Platform.All;

    /// <summary>
    /// Configures the dependency injection container by registering packet-related services.
    /// This method is called during application startup to set up the packet system.
    /// </summary>
    /// <param name="services">The container builder used to register application services.</param>
    public void OnBuildServices(ContainerBuilder services)
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

    /// <summary>
    /// Performs any additional configuration required after services have been built.
    /// This injector requires no additional configuration beyond service registration.
    /// </summary>
    /// <param name="provider">The service provider containing all registered application services.</param>
    public void OnConfigureServices(IServiceProvider provider)
    {
        // No additional configuration required for the packet system.
    }
}