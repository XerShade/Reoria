using LiteNetLib;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Reoria.Engine.Network.Sockets;

public class ClientSocket : Socket
{
    public ClientSocket(ILogger<ClientSocket> logger)
        : base(logger)
    {

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