using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application.Enumerations;
using GameBase = Microsoft.Xna.Framework.Game;

namespace Reoria.Engine.Application.Phases;

/// <summary>
/// Base interface for all game loop phase participants.
/// Game loop phase runs after DI container is built - full DI support via constructor injection.
/// </summary>
public interface IGameLoopPhase : IPhaseParticipant
{
    // All properties inherited from IPhaseParticipant
}

/// <summary>
/// Game loop phase for initializing graphics during game startup.
/// Runs during graphics initialization - no DI dependencies allowed in the method signature.
/// </summary>
public interface IGameInitializeGraphics : IGameLoopPhase
{
    /// <summary>
    /// Called during graphics initialization to set up graphics-related services.
    /// This method receives the GraphicsDeviceManager and GraphicsDevice directly.
    /// </summary>
    /// <param name="services">The container builder for registering graphics services.</param>
    /// <param name="graphicsDeviceManager">The graphics device manager.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    void OnInitializeGraphics(ContainerBuilder services, GraphicsDeviceManager graphicsDeviceManager, GraphicsDevice graphicsDevice);
}

/// <summary>
/// Game loop phase for loading content during game startup.
/// Runs during content loading - no DI dependencies allowed in the method signature.
/// </summary>
public interface IGameLoadContent : IGameLoopPhase
{
    /// <summary>
    /// Called during content loading to set up content-related services.
    /// This method receives the ContentManager directly.
    /// </summary>
    /// <param name="services">The container builder for registering content services.</param>
    /// <param name="contentManager">The content manager.</param>
    void OnLoadContent(ContainerBuilder services, ContentManager contentManager);
}

/// <summary>
/// Game loop phase for final game initialization after content is loaded.
/// Runs after content loading - full DI support via constructor injection.
/// </summary>
public interface IGameInitialize : IGameLoopPhase
{
    /// <summary>
    /// Called after content loading for final game initialization.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="game">The game instance.</param>
    void OnInitializeGame(GameBase game);
}

/// <summary>
/// Game loop phase for handling input every frame.
/// Runs every frame during input processing - full DI support via constructor injection.
/// </summary>
public interface IGameInput : IGameLoopPhase
{
    /// <summary>
    /// Called every frame to handle input.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="keyboard">Current state of the keyboard.</param>
    /// <param name="mouse">Current state of the mouse.</param>
    void OnHandleInput(GameTime gameTime, KeyboardState keyboard, MouseState mouse);
}

/// <summary>
/// Game loop phase for variable time updates every frame.
/// Runs every frame during variable update - full DI support via constructor injection.
/// </summary>
public interface IGameVariableUpdate : IGameLoopPhase
{
    /// <summary>
    /// Called every frame for variable time updates.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    void OnVariableUpdate(GameTime gameTime);
}

/// <summary>
/// Game loop phase for fixed time updates.
/// Runs at fixed time intervals - full DI support via constructor injection.
/// </summary>
public interface IGameFixedUpdate : IGameLoopPhase
{
    /// <summary>
    /// Called at fixed time intervals for fixed time updates.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    void OnFixedUpdate(GameTime gameTime);
}

/// <summary>
/// Game loop phase for late update operations.
/// Runs after all Update logic, useful for post-processing, camera following, UI updates, etc.
/// Runs every frame during late update - full DI support via constructor injection.
/// </summary>
public interface IGameLateUpdate : IGameLoopPhase
{
    /// <summary>
    /// Called every frame after all Update logic has run.
    /// Useful for post-processing, camera following, UI updates, and other late-stage processing.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    void OnLateUpdate(GameTime gameTime);
}

/// <summary>
/// Game loop phase for pre-rendering operations.
/// Runs every frame before main rendering - full DI support via constructor injection.
/// </summary>
public interface IGamePreRender : IGameLoopPhase
{
    /// <summary>
    /// Called every frame before main rendering.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="spriteBatch">The sprite batch for rendering.</param>
    void OnPreRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}

/// <summary>
/// Game loop phase for main rendering operations.
/// Runs every frame during main rendering - full DI support via constructor injection.
/// </summary>
public interface IGameRender : IGameLoopPhase
{
    /// <summary>
    /// Called every frame for main rendering.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="spriteBatch">The sprite batch for rendering.</param>
    void OnRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}

/// <summary>
/// Game loop phase for post-rendering operations.
/// Runs every frame after main rendering - full DI support via constructor injection.
/// </summary>
public interface IGamePostRender : IGameLoopPhase
{
    /// <summary>
    /// Called every frame after main rendering.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="spriteBatch">The sprite batch for rendering.</param>
    void OnPostRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}