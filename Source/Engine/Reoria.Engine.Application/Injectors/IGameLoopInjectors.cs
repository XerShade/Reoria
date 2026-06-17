using Microsoft.Xna.Framework;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Base interface for all game loop injectors providing common metadata.
/// </summary>
/// <remarks>
/// Game loop injectors are used to hook into the game loop phases such as update,
/// draw, and input handling. They are completely decoupled from application-level
/// injectors and focus solely on game loop functionality.
/// </remarks>
public interface IGameLoopInjector : IInjector
{
    // All base properties are inherited from IInjector
}

/// <summary>
/// Defines an abstraction contract for an injector that can participate in variable update game loop events.
/// </summary>
/// <remarks>
/// Variable update is called every frame and should be used for game logic that
/// needs to run continuously but doesn't require fixed timing. This includes things
/// like animation, AI behavior, and user interface updates.
/// </remarks>
public interface IVariableUpdateInjector : IGameLoopInjector
{
    /// <summary>
    /// Called on a variable timescale during the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <remarks>
    /// This method is called every frame with the actual elapsed time since the last frame.
    /// Use gameTime.ElapsedGameTime for frame-dependent calculations.
    /// </remarks>
    void OnVariableUpdate(GameTime gameTime);
}

/// <summary>
/// Defines an abstraction contract for an injector that can participate in fixed update game loop events.
/// </summary>
/// <remarks>
/// Fixed update is called at a consistent rate (typically 60 times per second) and should
/// be used for physics calculations, network synchronization, and other time-sensitive
/// operations that require predictable timing.
/// </remarks>
public interface IFixedUpdateInjector : IGameLoopInjector
{
    /// <summary>
    /// Called on a fixed timescale during the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <remarks>
    /// This method is called at a fixed interval regardless of frame rate.
    /// Use gameTime.ElapsedGameTime which will be consistent for fixed update calculations.
    /// </remarks>
    void OnFixedUpdate(GameTime gameTime);
}
