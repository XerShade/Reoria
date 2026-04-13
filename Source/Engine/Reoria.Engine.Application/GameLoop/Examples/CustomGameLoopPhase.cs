using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

namespace Reoria.Engine.Application.GameLoop.Examples;

/// <summary>
/// Example of a custom game loop phase that demonstrates how to extend the game loop.
/// This phase could handle game-specific logic like physics, AI, or custom systems.
/// </summary>
public class CustomGameLoopPhase : IGameLoopPhase
{
    private readonly ILogger<CustomGameLoopPhase> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomGameLoopPhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public CustomGameLoopPhase(ILogger<CustomGameLoopPhase> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "Custom Game Logic";

    /// <inheritdoc />
    public int Priority => 75; // Runs after network updates but before injectors

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        // Example: Only run on fixed updates for physics simulation
        if (context.IsFixedUpdate)
        {
            this.logger.LogDebug("Running physics simulation for tick {TickNumber}", context.TickNumber);
            
            // Add your physics simulation logic here
            // Example: physicsSystem.Update(context.FixedStep);
        }
        else
        {
            this.logger.LogDebug("Running variable game logic for tick {TickNumber}", context.TickNumber);
            
            // Add your variable update logic here
            // Example: gameLogic.Update(context.GameTime);
        }

        // Example of using context properties to share data between phases
        if (!context.Properties.ContainsKey("CustomPhaseExecuted"))
        {
            context.Properties["CustomPhaseExecuted"] = true;
        }

        await Task.CompletedTask;
    }
}
