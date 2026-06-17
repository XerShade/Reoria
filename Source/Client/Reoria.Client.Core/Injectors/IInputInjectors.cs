using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Client.Core.Injectors;

/// <summary>
/// Defines a contract for components that handle input injection into the game loop.
/// </summary>
public interface IInputInjector : IInjector
{
    /// <summary>
    /// Called during the input handling phase of the game loop to process keyboard and mouse input.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing state.</param>
    /// <param name="keyboard">Current state of the keyboard.</param>
    /// <param name="mouse">Current state of the mouse.</param>
    void OnHandleInput(GameTime gameTime, KeyboardState keyboard, MouseState mouse);
}