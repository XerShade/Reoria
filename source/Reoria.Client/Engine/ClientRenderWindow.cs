using Reoria.Engine.Interfaces;
using Reoria.Game.Data;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;

namespace Reoria.Client.Engine;
public class ClientRenderWindow : RenderWindow
{
    private ClientThread? engineThread;

    public ClientRenderWindow(nint handle) : base(handle) => this.SetupRenderWindow();
    public ClientRenderWindow(VideoMode mode, string title) : base(mode, title) => this.SetupRenderWindow();
    public ClientRenderWindow(nint handle, ContextSettings settings) : base(handle, settings) => this.SetupRenderWindow();
    public ClientRenderWindow(VideoMode mode, string title, Styles style) : base(mode, title, style) => this.SetupRenderWindow();
    public ClientRenderWindow(VideoMode mode, string title, Styles style, ContextSettings settings) : base(mode, title, style, settings) => this.SetupRenderWindow();

    public virtual ClientRenderWindow AttachToThread(ClientThread engineThread)
    {
        this.engineThread = engineThread;

        return this;
    }

    private void SetupRenderWindow()
    {
        this.SetVisible(false);
        this.Closed += (s, e) =>
        {
            this.Close();
            this.engineThread?.Stop();
        };
        this.Clear(new Color(100, 149, 237));
        this.Display();
    }

    public virtual void HandleInput()
    {
        float movement_speed = Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 8f : 4f;
        Vector2f velocity = new();

        if (Keyboard.IsKeyPressed(Keyboard.Key.W))
        {
            velocity = new Vector2f(velocity.X, velocity.Y - movement_speed);
        }
        if (Keyboard.IsKeyPressed(Keyboard.Key.S))
        {
            velocity = new Vector2f(velocity.X, velocity.Y + movement_speed);
        }
        if (Keyboard.IsKeyPressed(Keyboard.Key.A))
        {
            velocity = new Vector2f(velocity.X - movement_speed, velocity.Y);
        }
        if (Keyboard.IsKeyPressed(Keyboard.Key.D))
        {
            velocity = new Vector2f(velocity.X + movement_speed, velocity.Y);
        }

        if (velocity.X != 0 || velocity.Y != 0)
        {
            this.engineThread?.Networking.SendMoveCommand(velocity);
        }
    }

    public virtual void RenderPlayers(IEnumerable<Player> players)
    {
        RectangleShape shape = new(new Vector2f(32, 32))
        {
            FillColor = Color.Green,
        };

        foreach (Player player in players)
        {
            shape.Position = new Vector2f(player.X, player.Y);
            this.Draw(shape);
        }
    }
}
