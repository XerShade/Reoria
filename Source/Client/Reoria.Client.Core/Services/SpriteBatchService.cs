using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Client.Core.Injectors;
using Reoria.Client.Core.Services.Interfaces;
using Reoria.Engine.Application.Enumerations;

namespace Reoria.Client.Core.Services;

public class SpriteBatchService : ISpriteBatchService, IPreDrawingInjector, IPostDrawingInjector, ILifeCycleInitializeGraphicsInjector
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

    public void OnPreDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        => spriteBatch.Begin();

    public void OnPostDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        => spriteBatch.End();
}