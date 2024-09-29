using Reoria.Engine;
using System.Net.Sockets;

namespace Reoria.Client.Engine;

public class ClientThread : EngineThread
{
    private TcpClient? tcpClient;

    protected override void OnThreadStart()
    {
        try
        {
            this.tcpClient = new TcpClient("localhost", 5555);
            Console.WriteLine($"Connected to server at {this.tcpClient.Client.RemoteEndPoint}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to server: {ex.Message}");
        }

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        if (this.tcpClient != null)
        {
            this.tcpClient.Close();
            Console.WriteLine("Disconnecting from the server.");
        }

        base.OnThreadStop();
    }
}
