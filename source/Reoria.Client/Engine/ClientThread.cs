using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine;
using SFML.Graphics;

namespace Reoria.Client.Engine;

public class ClientThread(ClientShared shared, ClientNetEventListener netEventListener, ClientRenderWindow renderWindow, ILogger<ClientThread> logger, IConfigurationRoot configuration, int ticksPerSecond = 60) : EngineThread(logger, configuration, ticksPerSecond)
{
    public readonly ClientShared Shared = shared;
    public readonly ClientNetEventListener Networking = netEventListener;
    public readonly ClientRenderWindow RenderWindow = renderWindow;

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

        this.RenderWindow.RenderPlayers(this.Shared.Players.Values);

        this.RenderWindow.Display();

        base.OnThreadTick();
    }
}
