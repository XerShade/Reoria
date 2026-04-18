using Microsoft.Xna.Framework;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;

namespace Reoria.Engine.Application.GameLoop.Interfaces;

/// <summary>
/// Defines the contract for a game loop that manages the update cycle.
/// </summary>
/// <remarks>
/// This interface provides the foundation for game loop management in Reoria engine.
/// The game loop is responsible for coordinating the execution of game logic at regular intervals,
/// managing both fixed and variable update cycles, and organizing the execution of game phases.
/// It supports both client-side rendering loops and server-side simulation loops.
/// </remarks>
public interface IGameLoop : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the game loop is running.
    /// </summary>
    /// <remarks>
    /// Indicates the active state of the game loop. When true, the loop will process
    /// ticks on schedule. When false, the loop is paused or stopped and will not
    /// execute game logic or phases. This property is thread-safe for status checks.
    /// </remarks>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the collection of game loop phases.
    /// </summary>
    /// <remarks>
    /// Contains all registered phases that execute in order during each tick.
    /// Phases are organized by priority and can be enabled/disabled individually.
    /// Common phases include input handling, physics updates, AI processing, and rendering.
    /// The collection is read-only after initialization to prevent runtime modifications.
    /// </remarks>
    IList<IGameLoopPhase> Phases { get; }

    /// <summary>
    /// Starts the game loop.
    /// </summary>
    /// <remarks>
    /// Initializes the game loop state and begins processing ticks.
    /// This method resets timing accumulators, starts any background timers,
    /// and enables phase execution. The loop will continue running until Stop() is called
    /// or the application requests termination. This method is idempotent and safe to call
    /// multiple times.
    /// </remarks>
    void Start();

    /// <summary>
    /// Stops the game loop.
    /// </summary>
    /// <remarks>
    /// Gracefully terminates the game loop and stops phase execution.
    /// This method signals any cancellation tokens, stops background processing,
    /// and performs cleanup of loop-specific resources. The loop will finish processing
    /// the current tick before stopping completely. This method is thread-safe.
    /// </remarks>
    void Stop();

    /// <summary>
    /// Processes a single tick of the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information for this tick.</param>
    /// <remarks>
    /// Executes one complete cycle of the game loop, including timing calculations,
    /// fixed update processing, and phase execution. The gameTime parameter provides
    /// timing information for frame-rate independent updates and delta time calculations.
    /// This method should be called regularly by the application's main loop.
    /// If the loop is not running, this method will return without processing.
    /// </remarks>
    void Tick(GameTime gameTime);
}
