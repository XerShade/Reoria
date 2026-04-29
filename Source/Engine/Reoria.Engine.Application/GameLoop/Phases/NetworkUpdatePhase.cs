using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;
using Reoria.Engine.Network.Sockets;

namespace Reoria.Engine.Application.GameLoop.Phases;

/// <summary>
/// A game loop phase that handles network socket updates during each game tick.
/// This phase ensures that network communication is processed regularly as part of the game loop.
/// </summary>
public class NetworkUpdatePhase : IGameLoopPhase
{
    private readonly ILogger<NetworkUpdatePhase> logger;
    private readonly Socket socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkUpdatePhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="socket">The socket to update.</param>
    public NetworkUpdatePhase(ILogger<NetworkUpdatePhase> logger, Socket socket)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.socket = socket ?? throw new ArgumentNullException(nameof(socket));
    }

    /// <inheritdoc />
    public string Name => "Network Update";

    /// <inheritdoc />
    public int Priority => 100;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsAsync => true;

    /// <inheritdoc />
    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update the network socket
            this.socket.Update();

            this.logger.LogTrace("Network socket updated successfully for tick {TickNumber}", context.TickNumber);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to update network socket for tick {TickNumber}", context.TickNumber);
            throw;
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Execute(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update the network socket
            this.socket.Update();

            this.logger.LogTrace("Network socket updated successfully for tick {TickNumber}", context.TickNumber);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to update network socket for tick {TickNumber}", context.TickNumber);
            throw;
        }

        return Task.CompletedTask;
    }
}