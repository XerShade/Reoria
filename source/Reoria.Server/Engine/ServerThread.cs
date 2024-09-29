using Reoria.Engine;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Server.Engine;

public class ServerThread : EngineThread
{
    private readonly TcpListener listener = new(IPAddress.Any, 5555);

    protected override void OnThreadStart()
    {
        this.listener.Start();

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.listener.Stop();

        base.OnThreadStop();
    }

    protected override void OnThreadTick()
    {
        this.HandleIncomingConnections();

        base.OnThreadTick();
    }

    private void HandleIncomingConnections()
    {
        if (this.listener.Pending())
        {
            TcpClient client = this.listener.AcceptTcpClient();
            Console.WriteLine($"Received new client connection from {client.Client.RemoteEndPoint}");
        }
    }
}
