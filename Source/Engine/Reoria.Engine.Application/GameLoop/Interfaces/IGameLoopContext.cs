using Microsoft.Xna.Framework;

namespace Reoria.Engine.Application.GameLoop.Interfaces;

/// <summary>
/// Provides context information for a game loop tick.
/// </summary>
public interface IGameLoopContext
{
    /// <summary>
    /// Gets the game time information for this tick.
    /// </summary>
    GameTime GameTime { get; }

    /// <summary>
    /// Gets the current tick number.
    /// </summary>
    long TickNumber { get; }

    /// <summary>
    /// Gets the elapsed time since the game loop started.
    /// </summary>
    TimeSpan TotalElapsedTime { get; }

    /// <summary>
    /// Gets a value indicating whether this is a fixed update tick.
    /// </summary>
    bool IsFixedUpdate { get; }

    /// <summary>
    /// Gets the fixed step time for fixed updates.
    /// </summary>
    TimeSpan FixedStep { get; }

    /// <summary>
    /// Gets the number of fixed update steps that have been executed this tick.
    /// </summary>
    int FixedStepCount { get; }

    /// <summary>
    /// Gets or sets custom properties for the context.
    /// </summary>
    IDictionary<string, object> Properties { get; }
}
