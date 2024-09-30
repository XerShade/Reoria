using Reoria.Engine;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Reoria.Server.Engine;

public class ServerThread : EngineThread
{
    private readonly TcpListener listener = new(IPAddress.Any, 5555);

    protected override void OnThreadStart()
    {
        this.listener.Start();
        Console.WriteLine($"Started server on {this.listener.LocalEndpoint}");

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.listener.Stop();
        Console.WriteLine($"Stopped listening on {this.listener.LocalEndpoint}");

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

            Thread clientThread = new(() => this.HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[256];

        try
        {
            while (this.IsRunning)
            {
                if (this.IsPaused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received from {client.Client.RemoteEndPoint}: {message}");

                    byte[] response = Encoding.ASCII.GetBytes($"Echo: {message}");
                    stream.Write(response, 0, response.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occured on client thread from {client.Client.RemoteEndPoint}: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}
