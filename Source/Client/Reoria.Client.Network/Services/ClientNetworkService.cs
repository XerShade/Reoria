using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Client.Network.Sockets;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Network.Services;

/// <summary>
/// Game loop phase participant that manages updating the client network socket during the variable update loop.
/// </summary>
public class ClientNetworkService(ILogger<ClientNetworkService> logger, ClientSocket socket) : IGameVariableUpdate, IApplicationStop
{
    public string Name => "Client Network Service";

    public string Description => "Updates client networking and packet processing during the game loop.";

    public Type[] Dependencies => [];

    public Platform Platform => Platform.All & ~Platform.Server;

    protected ILogger<ClientNetworkService> Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    protected ClientSocket Socket { get; } = socket ?? throw new ArgumentNullException(nameof(socket));

    public void OnVariableUpdate(GameTime gameTime)
    {
        if (this.Socket.IsRunning)
        {
            this.Socket.Update();
        }
    }

    public void OnApplicationStop()
    {
        try
        {
            if (this.Socket.IsRunning)
            {
                this.Logger.LogInformation("Stopping client network socket on application shutdown...");
                this.Socket.Stop();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error while stopping client network socket.");
        }
    }
}
