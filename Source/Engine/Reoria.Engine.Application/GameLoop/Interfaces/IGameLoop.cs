using Microsoft.Xna.Framework;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;

namespace Reoria.Engine.Application.GameLoop.Interfaces;

/// <summary>
/// Defines the contract for a game loop that manages the update cycle.
/// </summary>
public interface IGameLoop : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the game loop is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the collection of game loop phases.
    /// </summary>
    IList<IGameLoopPhase> Phases { get; }

    /// <summary>
    /// Starts the game loop.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the game loop.
    /// </summary>
    void Stop();

    /// <summary>
    /// Processes a single tick of the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information for this tick.</param>
    void Tick(GameTime gameTime);
}
