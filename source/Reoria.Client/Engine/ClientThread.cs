using Reoria.Engine;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Reoria.Client.Engine;

public class ClientThread : EngineThread
{
    private TcpClient? tcpClient;
    private NetworkStream? networkStream;
    private RenderWindow? renderWindow;

    private RectangleShape? player;
    private Vector2f? lastPosition;

    protected override void OnThreadStart()
    {
        this.renderWindow = new RenderWindow(new VideoMode(1280, 720), "Reoria");
        this.renderWindow.Closed += (s, e) =>
        {
            this.renderWindow.Close();
            this.Stop();
        };
        this.renderWindow.Clear(new Color(100, 149, 237));
        this.renderWindow.Display();

        this.player = new RectangleShape(new Vector2f(32, 32))
        {
            FillColor = Color.Green,
            Position = new Vector2f((this.renderWindow.Size.X / 2) - 16, (this.renderWindow.Size.Y / 2) - 16)
        };

        try
        {
            this.tcpClient = new TcpClient();
            this.tcpClient.Connect("localhost", 5555);
            this.networkStream = this.tcpClient.GetStream();
            Console.WriteLine($"Connected to server at {this.tcpClient.Client.RemoteEndPoint}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to server: {ex.Message}");
            this.Stop();
        }

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        if (this.tcpClient is not null)
        {
            this.tcpClient?.Close();
            this.networkStream?.Close();
            Console.WriteLine("Disconnecting from the server.");
        }

        base.OnThreadStop();
    }

    protected override void OnThreadTick()
    {
        if(this.renderWindow is null)
        {
            this.Stop();
            base.OnThreadTick();
            return;
        }

        this.renderWindow.DispatchEvents();

        this.HandleInput(this.player);

        this.SendPlayerPosition(this.player);

        this.HandleServerResponse();

        this.renderWindow.Clear(new Color(100, 149, 237));
        this.renderWindow.Draw(this.player);
        this.renderWindow.Display();

        base.OnThreadTick();
    }

    private void HandleInput(RectangleShape? player)
    {
        float movement_speed = Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 8f : 4f;

        if (player is not null)
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                player.Position = new Vector2f(player.Position.X, player.Position.Y - movement_speed);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                player.Position = new Vector2f(player.Position.X, player.Position.Y + movement_speed);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                player.Position = new Vector2f(player.Position.X - movement_speed, player.Position.Y);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                player.Position = new Vector2f(player.Position.X + movement_speed, player.Position.Y);
            }
        }
    }

    private void HandleServerResponse()
    {
        string message = this.ReceiveFromServer();
        if (!string.IsNullOrWhiteSpace(message))
        {
            if (!message.StartsWith("Echo:")) {
                Console.WriteLine($"Received message from server: {message}");
            }
        }
    }

    private string ReceiveFromServer()
    {
        if (this.networkStream is not null)
        {
            if(this.networkStream.DataAvailable)
            {
                byte[] buffer = new byte[256];
                int bytesRead = this.networkStream.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
        }

        return string.Empty;
    }

    private void SendPlayerPosition(RectangleShape? player)
    {
        if (player is not null)
        {
            if (this.lastPosition != player.Position)
            {
                if (this.tcpClient is not null)
                {
                    if (this.tcpClient.Connected)
                    {
                        if (this.networkStream is not null)
                        {
                            string message = $"Moved Postion: {player.Position.X}, {player.Position.Y}";
                            byte[] response = Encoding.ASCII.GetBytes($"{message}");
                            this.networkStream.Write(response, 0, response.Length);
                        }
                    }
                }

                this.lastPosition = player?.Position;
            }
        }
    }
}
