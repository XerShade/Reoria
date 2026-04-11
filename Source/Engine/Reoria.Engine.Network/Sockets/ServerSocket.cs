using Autofac;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Modules;

namespace Reoria.Engine.Network.Sockets;

public class ServerSocketInjector : IApplicationServicesModule
{
    public string Name
        => "ServerSocket Injector";

    public string Description
        => "Injects ServerSocket to the application.";

    public Type[] Dependencies
        => [];

    public void OnGetServices(ContainerBuilder services)
        => services.RegisterType<ServerSocket>().As<ServerSocket>().SingleInstance();

    public void OnConfigureServices(IServiceProvider provider)
    { }
}

public class ServerSocket : Socket
{
    public ServerSocket(ILogger<ServerSocket> logger)
        : base(logger)
    {

    }

    protected override void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.LogInformation("Recieved connection request from {Address}.", request.RemoteEndPoint.Address.ToString());

        if (this.Manager.ConnectedPeersCount < 10)
        {
            NetPeer peer = request.AcceptIfKey("Reoria");

            if(peer != null)
            {
                this.Logger.LogInformation("Connection request from {Address} was accepted.", request.RemoteEndPoint.Address.ToString());
            }
            else
            {
                this.Logger.LogWarning("Connection request from {Address} was rejected.", request.RemoteEndPoint.Address.ToString());
            }
        }
        else
        {
            this.Logger.LogWarning("Connection request from {Address} was rejected, the server is full.", request.RemoteEndPoint.Address.ToString());
            request.Reject();
        }
    }

    public override void Start()
        => this.Manager.Start(7234);

    public override void Stop()
        => this.Manager.Stop();
}