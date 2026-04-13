using Autofac;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using System.Diagnostics;

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

public class ClientSocket : Socket
{
    public ClientSocket(ILogger<ClientSocket> logger)
        : base(logger)
    {

    }

    public override void Start() 
        => this.Manager.Start();

    public override void Stop()
        => this.Manager.Stop();

    public virtual bool Connect(string address, int port, int timeout = 5)
    {
        NetPeer peer = this.Manager.Connect(address, port, "Reoria");
        Stopwatch sw = Stopwatch.StartNew();

        while (peer.ConnectionState is not ConnectionState.Connected and not ConnectionState.Disconnected)
        {
            if (sw.Elapsed.TotalSeconds > timeout)
            {
                this.Manager.DisconnectAll();
                return false;
            }
            System.Threading.Thread.Sleep(1000);
        }
        sw.Stop();

        return peer.ConnectionState == ConnectionState.Connected;
    }
}