using Reoria.Engine;
using Reoria.Game.Data;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Reoria.Client.Engine;

public class ClientThread : EngineThread
{
    public readonly ClientNetEventListener Networking = new();
    public readonly ClientRenderWindow RenderWindow = new(new VideoMode(1280, 720), "Reoria");
    public readonly Dictionary<int, Player> Players = [];
    public int LocalPlayerId { get; internal set; } = 0;

    protected override void OnThreadStart()
    {
        this.RenderWindow.AttachToThread(this).SetVisible(true);
        this.Networking.AttachToThread(this).Start("localhost", 5555);

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.Networking?.Stop();

        this.RenderWindow?.SetVisible(false);
        this.RenderWindow?.Dispose();

        base.OnThreadStop();
    }

    protected override void OnThreadTick()
    {
        if(!this.RenderWindow.IsOpen)
        {
            this.Stop();
            base.OnThreadTick();
            return;
        }

        this.RenderWindow.DispatchEvents();

        if (this.RenderWindow.HasFocus())
        {
            this.RenderWindow.HandleInput();
        }

        this.Networking.PollEvents();

        this.RenderWindow.Clear(new Color(100, 149, 237));

        this.RenderWindow.RenderPlayers(this.Players.Values);

        this.RenderWindow.Display();

        base.OnThreadTick();
    }
}
