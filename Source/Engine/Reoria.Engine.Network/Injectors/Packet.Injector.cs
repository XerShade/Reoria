using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Network.Packets;
using Reoria.Engine.Network.Packets.Interfaces;
using System.Reflection;

namespace Reoria.Engine.Network.Injectors;

public class PacketInjector : IApplicationServicesInjector
{
    public string Name 
        => "Packet System";

    public string Description 
        => "Adds packet functionality to the game.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All;

    public void OnBuildServices(ContainerBuilder services)
    {
        _ = services.RegisterType<PacketManager>()
            .As<IPacketManager>()
            .SingleInstance();

        this.DiscoverIncomingPackets(services);
        this.DiscoverOutgoingPackets(services);
    }

    protected virtual void DiscoverIncomingPackets(ContainerBuilder services)
    {
        // Discover the application injectors.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IIncomingPacket).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Iterate through the types and register them.
        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IIncomingPacket>()
                .InstancePerDependency();
        }
    }

    protected virtual void DiscoverOutgoingPackets(ContainerBuilder services)
    {
        // Discover the application injectors.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IOutgoingPacket).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Iterate through the types and register them.
        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IOutgoingPacket>()
                .InstancePerDependency();
        }
    }

    public void OnConfigureServices(IServiceProvider provider)
    {
        // Requires no additional configuration.
    }
}