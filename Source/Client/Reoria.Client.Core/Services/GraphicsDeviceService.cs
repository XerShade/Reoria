using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Color = Microsoft.Xna.Framework.Color;
using IGraphicsDeviceService = Reoria.Client.Core.Services.Interfaces.IGraphicsDeviceService;

namespace Reoria.Client.Core.Services;

/// <summary>
/// Game loop phase participant that provides graphics device management and rendering setup.
/// </summary>
/// <remarks>
/// This phase participant handles graphics device initialization and pre-render setup.
/// For graphics initialization, it uses method parameters (no DI dependencies).
/// For rendering, it has full DI support via constructor injection.
/// </remarks>
public class GraphicsDeviceService : IGraphicsDeviceService, IGameInitializeGraphics, IGamePreRender
{
    public string Name
        => "Graphics Device Service";

    public string Description
        => "Adds support for configuring the graphics device and rendering actions during drawing.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All & ~Platform.Server;

    public GraphicsDevice? GraphicsDevice { get; private set; }

    public GraphicsDeviceManager? GraphicsDeviceManager { get; private set; }

    public void OnInitializeGraphics(ContainerBuilder services, GraphicsDeviceManager graphicsDeviceManager, GraphicsDevice graphicsDevice)
    {
        // Register the graphics device manager with proper lifetime (not singleton to prevent memory leaks)
        _ = services.RegisterInstance<GraphicsDeviceManager>(graphicsDeviceManager)
            .Keyed<GraphicsDeviceManager>("GraphicsDeviceManager")
            .As<GraphicsDeviceManager>()
            .SingleInstance();

        // Register the graphics device with proper lifetime
        _ = services.RegisterInstance<GraphicsDevice>(graphicsDevice)
            .Keyed<GraphicsDevice>("GraphicsDevice")
            .As<GraphicsDevice>()
            .SingleInstance();

        // Store the graphics device and graphics device manager.
        this.GraphicsDevice = graphicsDevice;
        this.GraphicsDeviceManager = graphicsDeviceManager;
    }

    public void OnPreRender(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        // Set up the graphics device for drawing
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        // Clear the screen
        graphicsDevice.Clear(Color.CornflowerBlue);
    }
}