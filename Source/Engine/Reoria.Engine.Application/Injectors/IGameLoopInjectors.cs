using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

/// <summary>
/// Defines an abstraction contract for an injector that can participate in rendering operations.
/// </summary>
/// <remarks>
/// Render injectors are called during the render phase and should be used for
/// all rendering operations including sprites, text, UI elements, and graphics effects.
/// This interface combines pre-draw, draw, and post-draw operations into a single
/// simplified interface that follows MonoGame's standard rendering pattern.
/// </remarks>
public interface IDrawingInjector : IGameLoopInjector
{
    /// <summary>
    /// Called during the render phase of the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <param name="graphicsDevice">The graphics device for rendering operations.</param>
    /// <param name="spriteBatch">The sprite batch for drawing operations (may be null).</param>
    /// <remarks>
    /// This method is called after update phases and should contain all rendering code.
    /// The graphics device is properly configured and the sprite batch is managed by the caller.
    /// Use gameTime.TotalGameTime for time-based animations and effects.
    /// </remarks>
    void OnDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}

/// <summary>
/// Defines an abstraction contract for an injector that can participate in input handling.
/// </summary>
/// <remarks>
/// Input injectors are called during the input phase and should be used for processing
/// user input from keyboard, mouse, gamepad, or other input devices.
/// </remarks>
public interface IInputInjector : IGameLoopInjector
{
    /// <summary>
    /// Called during the input handling phase of the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information.</param>
    /// <remarks>
    /// This method is called before update phases and should handle input processing
    /// such as checking key states, mouse movements, and gamepad input.
    /// </remarks>
    void OnInput(GameTime gameTime);
}


/// <summary>
/// Combined interface for injectors that participate in all game loop events.
/// </summary>
/// <remarks>
/// Implement this interface if you need to participate in all phases of the game loop.
/// This is useful for comprehensive game systems that need to handle input,
/// update logic, and rendering all in one class.
/// </remarks>
public interface ICompleteGameLoopInjector : IVariableUpdateInjector, IFixedUpdateInjector, IDrawingInjector, IInputInjector
{
    // This interface combines all the game loop related interfaces
    // No additional methods needed as they're inherited from the base interfaces
}