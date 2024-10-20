﻿using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;
using SFML.Graphics;

namespace Reoria.Client.Engine;

public class ClientThread(IServiceProvider services, int ticksPerSecond = 60) : EngineThread(services, ticksPerSecond)
{
    public readonly ClientNetEventListener Networking = services.GetRequiredService<ClientNetEventListener>();
    public readonly ClientRenderWindow RenderWindow = services.GetRequiredService<ClientRenderWindow>();

    protected override void OnThreadStart()
    {
        this.RenderWindow.SetVisible(true);
        this.Networking.Start();

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.Networking?.Stop();

        this.RenderWindow?.SetVisible(false);
        this.RenderWindow?.Dispose();

        base.OnThreadStop();
    }

    protected override void OnThreadDynamicTick(float deltaTime)
    {
        if(!this.RenderWindow.IsOpen)
        {
            this.Stop();
            base.OnThreadDynamicTick(deltaTime);
            return;
        }

        this.RenderWindow.DispatchEvents();

        if (this.RenderWindow.HasFocus())
        {
            this.RenderWindow.HandleInput();
        }

        this.Networking.PollEvents();

        this.RenderWindow.Clear(new Color(100, 149, 237));

        this.RenderWindow.RenderPlayers();

        this.RenderWindow.Display();

        base.OnThreadDynamicTick(deltaTime);
    }
}
