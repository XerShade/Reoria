using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Client.Core.Injectors;

/// <summary>
/// Defines a contract for components that inject drawing operations during the main draw phase of the game loop.
/// </summary>
public interface IDrawingInjector : IInjector
{
    /// <summary>
    /// Called during the main drawing phase to render content to the screen.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="spriteBatch">The sprite batch used for 2D rendering.</param>
    void OnDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}

/// <summary>
/// Defines a contract for components that inject drawing operations before the main draw phase of the game loop.
/// </summary>
public interface IPreDrawingInjector : IInjector
{
    /// <summary>
    /// Called before the main drawing phase to render content that should appear behind other elements.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="spriteBatch">The sprite batch used for 2D rendering.</param>
    void OnPreDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}

/// <summary>
/// Defines a contract for components that inject drawing operations after the main draw phase of the game loop.
/// </summary>
public interface IPostDrawingInjector : IInjector
{
    /// <summary>
    /// Called after the main drawing phase to render content that should appear on top of other elements.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="spriteBatch">The sprite batch used for 2D rendering.</param>
    void OnPostDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
}