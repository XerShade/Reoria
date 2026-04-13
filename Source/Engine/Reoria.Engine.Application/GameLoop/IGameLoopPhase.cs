using Microsoft.Xna.Framework;

namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// Defines the contract for a phase within the game loop.
/// </summary>
public interface IGameLoopPhase
{
    /// <summary>
    /// Gets the name of this phase.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the priority of this phase (higher values execute first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this phase is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Executes the phase logic.
    /// </summary>
    /// <param name="context">The game loop context for this tick.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default);
}
