using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Client.Core.Injectors;
using Reoria.Engine.Application.Enumerations;
using Color = Microsoft.Xna.Framework.Color;
using IGraphicsDeviceService = Reoria.Client.Core.Services.Interfaces.IGraphicsDeviceService;

namespace Reoria.Client.Core.Services;

public class GraphicsDeviceService : IGraphicsDeviceService, ILifeCycleInitializeGraphicsInjector, IPreDrawingInjector
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

    public void OnPreDraw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
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