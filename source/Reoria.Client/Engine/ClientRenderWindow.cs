using Reoria.Game.Data;
using Reoria.Game.Data.Interfaces;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Reoria.Client.Engine;

public class ClientRenderWindow : RenderWindow
{
    private readonly IGameData gameData;
    private readonly ClientNetEventListener network;

    public ClientRenderWindow(IGameData gameData, ClientNetEventListener network) : base(new VideoMode(1280, 720), "Reoria")
    {
        this.gameData = gameData;
        this.network = network;
        this.SetupRenderWindow();
    }

    private void SetupRenderWindow()
    {
        this.SetVisible(false);
        this.Closed += (s, e) => this.Close();
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
            this.network.SendMoveCommand(velocity);
        }
    }

    public virtual void RenderPlayers()
    {
        RectangleShape shape = new(new Vector2f(32, 32))
        {
            FillColor = Color.Green,
        };

        foreach (Player player in this.gameData.Players)
        {
            shape.Position = new Vector2f(player.X, player.Y);
            this.Draw(shape);
        }
    }
}
