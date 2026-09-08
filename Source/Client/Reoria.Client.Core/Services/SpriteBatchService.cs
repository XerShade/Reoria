using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Client.Core.Services.Interfaces;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Core.Services;

/// <summary>
/// Game loop phase participant that provides sprite batch management for rendering.
/// </summary>
/// <remarks>
/// This phase participant handles sprite batch initialization and render lifecycle management.
/// For graphics initialization, it uses method parameters (no DI dependencies).
/// For rendering, it has full DI support via constructor injection.
/// </remarks>
public class SpriteBatchService : ISpriteBatchService, IGamePreRender, IGamePostRender, IGameInitializeGraphics
{
    public string Name
        => "Sprite Batching Service";

    public string Description
        => "Adds support for configuring the sprite batch for drawing and executing rendering tasks.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All & ~Platform.Server;

    public virtual SpriteBatch? SpriteBatch { get; protected set; }

    public void OnInitializeGraphics(ContainerBuilder services, GraphicsDeviceManager graphicsDeviceManager, GraphicsDevice graphicsDevice)
    {
        // Create the sprite batch.
        this.SpriteBatch = new SpriteBatch(graphicsDevice);

        // Register the sprite batch with proper lifetime
        _ = services.RegisterInstance<SpriteBatch>(this.SpriteBatch ?? throw new ArgumentNullException("SpriteBatch is null."))
            .Keyed<SpriteBatch>("SpriteBatch")
            .As<SpriteBatch>()
            .SingleInstance();
    }

    public void OnPreRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        => spriteBatch.Begin();

    public void OnPostRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        => spriteBatch.End();
}