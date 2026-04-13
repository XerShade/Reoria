using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Engine.Network.Sockets;

public class ClientSocketInjector : IApplicationServicesInjector
{
    public string Name
        => "ClientSocket Injector";

    public string Description
        => "Injects ClientSocket to the application.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All & ~Platform.Server;

    public void OnGetServices(ContainerBuilder services)
        => services.RegisterType<ClientSocket>().As<ClientSocket>().SingleInstance();

    public void OnConfigureServices(IServiceProvider provider)
    { }
}