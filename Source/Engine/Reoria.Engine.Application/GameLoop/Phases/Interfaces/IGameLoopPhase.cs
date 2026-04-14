using Reoria.Engine.Application.GameLoop.Interfaces;

namespace Reoria.Engine.Application.GameLoop.Phases.Interfaces;

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
    /// Gets a value indicating whether this phase supports asynchronous execution.
    /// </summary>
    bool IsAsync { get; }

    /// <summary>
    /// Executes phase logic synchronously.
    /// </summary>
    /// <param name="context">The game loop context for this tick.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the synchronous operation.</returns>
    Task Execute(IGameLoopContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes phase logic asynchronously (optional override).
    /// </summary>
    /// <param name="context">The game loop context for this tick.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>Default implementation throws NotImplementedException. Override only if needed.</remarks>
    virtual Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default) => 
        throw new NotImplementedException($"{GetType().Name} does not support async execution");
}
