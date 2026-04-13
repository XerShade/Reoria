using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Reoria.Engine.Network.Sockets;

public class ClientSocket : Socket
{
    protected virtual string Address { get; init; }
    protected virtual int Port { get; init; }
    protected virtual int Timeout { get; init; }

    public ClientSocket(ILogger<ClientSocket> logger, IConfiguration configuration)
        : base(logger, configuration)
    {
        this.Address = configuration["Networking:Address"] ?? "127.0.0.1";
        this.Timeout = Convert.ToInt32(configuration["Networking:Timeout"] ?? "5");
    }

    public override bool Connect(string address, int port, int timeout = 5)
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