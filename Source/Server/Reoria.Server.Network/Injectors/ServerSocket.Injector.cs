using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Network.Sockets;
using Reoria.Server.Network.Sockets;

namespace Reoria.Server.Network.Injectors;

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

    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ServerSocket>().As<ServerSocket>().As<Socket>().SingleInstance();

    public void OnConfigureServices(IServiceProvider provider)
    { }
}