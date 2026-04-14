using Microsoft.Xna.Framework;
using Reoria.Engine.Application.GameLoop.Interfaces;

namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// A default implementation of game loop context.
/// </summary>
public class DefaultGameLoopContext : IGameLoopContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGameLoopContext"/> class.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <param name="tickNumber">The current tick number.</param>
    /// <param name="totalElapsedTime">The total elapsed time.</param>
    /// <param name="isFixedUpdate">Whether this is a fixed update.</param>
    /// <param name="fixedStep">The fixed step time.</param>
    /// <param name="fixedStepCount">The fixed step count.</param>
    public DefaultGameLoopContext(
        GameTime gameTime,
        long tickNumber,
        TimeSpan totalElapsedTime,
        bool isFixedUpdate,
        TimeSpan fixedStep,
        int fixedStepCount)
    {
        this.GameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
        this.TickNumber = tickNumber;
        this.TotalElapsedTime = totalElapsedTime;
        this.IsFixedUpdate = isFixedUpdate;
        this.FixedStep = fixedStep;
        this.FixedStepCount = fixedStepCount;
        this.Properties = new Dictionary<string, object>();
    }

    /// <inheritdoc />
    public GameTime GameTime { get; }

    /// <inheritdoc />
    public long TickNumber { get; }

    /// <inheritdoc />
    public TimeSpan TotalElapsedTime { get; }

    /// <inheritdoc />
    public bool IsFixedUpdate { get; }

    /// <inheritdoc />
    public TimeSpan FixedStep { get; }

    /// <inheritdoc />
    public int FixedStepCount { get; }

    /// <inheritdoc />
    public IDictionary<string, object> Properties { get; }
}
