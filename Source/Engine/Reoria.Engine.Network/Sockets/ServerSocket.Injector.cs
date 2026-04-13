using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Engine.Network.Sockets;

public class ServerSocketInjector : IApplicationServicesInjector
{
    public string Name
        => "ServerSocket Injector";

    public string Description
        => "Injects ServerSocket to the application.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.Server;

    public void OnGetServices(ContainerBuilder services)
        => services.RegisterType<ServerSocket>().As<ServerSocket>().SingleInstance();

    public void OnConfigureServices(IServiceProvider provider)
    { }
}