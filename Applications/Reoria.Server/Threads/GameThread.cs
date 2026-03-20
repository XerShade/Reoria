using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application.Threads;

namespace Reoria.Server.Threads;

public class GameThread(ILogger<IGameThread> logger) : IGameThread
{
    protected readonly ILogger<IGameThread> Logger = logger;

    /// <inheritdoc />
    public GameServiceContainer Services 
        => throw new NotImplementedException("The server does not have a game service container.");

    /// <inheritdoc />
    public void Run()
    {
        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation("Hello world!");
        }
    }
}