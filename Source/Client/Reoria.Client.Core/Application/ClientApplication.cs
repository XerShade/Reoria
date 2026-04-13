using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Network.Sockets;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Reoria.Client.Core.Application;

/// <summary>
/// Defines the client application and its functionality.
/// </summary>
public class ClientApplication : Game, IApplication
{
    /// <inheritdoc />
    public virtual Platform Platform { get; init; }
    /// <inheritdoc />
    public virtual ILogger<IApplication> Logger { get; init; }
    /// <inheritdoc />
    public virtual List<IApplicationInjector> Injectors { get; init; } = [];
    /// <inheritdoc />
    public virtual IConfiguration Configuration { get; init; }
    /// <inheritdoc />
    public virtual ILoggerFactory LoggerFactory { get; init; }
    /// <inheritdoc />
    public virtual ContainerBuilder ContainerBuilder { get; init; }
    /// <inheritdoc />
    public virtual IServiceProvider Provider { get; protected set; }

    /// <summary>
    /// Gets an instance of <see cref="GraphicsDeviceManager"/> to manage the graphics device.
    /// </summary>
    private GraphicsDeviceManager GraphicsDeviceManager { get; set; }
    /// <summary>
    /// Gets an instance of <see cref="SpriteBatch"/> to batch draw calls.
    /// </summary>
    private SpriteBatch? SpriteBatch { get; set; }
    /// <summary>
    /// Gets the accumulator for the fixed update loop.
    /// </summary>
    protected TimeSpan Accumulator { get; set; }
    /// <summary>
    /// Gets the fixed step for the fixed update loop.
    /// </summary>
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 30.0);
    /// <summary>
    /// Gets the maximum steps for the fixed update loop allowed per update cycle.
    /// </summary>
    protected int MaxSteps { get; init; } = 5;
    /// <summary>
    /// Gets the current number of steps for the fixed update loop.
    /// </summary>
    protected int Steps { get; set; } = 0;
    /// <summary>
    /// Gets an instance of <see cref="ClientSocket"/> to manage the networking functionality.
    /// </summary>
    protected ClientSocket Socket { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Constructs a new instance of <see cref="ClientApplication"/>.
    /// </summary>
    /// <param name="logger">A logger instance to log messages to.</param>
    /// <param name="args">The command line arguments.</param>
    /// <param name="context">The application boot context.</param>
    public ClientApplication(ILogger<IApplication> logger, [KeyFilter("CommandLineArgs")] string[] args, AppBootContext context)
    {
        // Store the platform.
        this.Platform = context.Platform;

        // Store the logger and report the initialization.
        this.Logger = logger;
        this.Logger.LogInformation("Initializing server application...");

        // Discover the application injectors.
        this.Injectors = this.DiscoverInjectors();

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.LoggerFactory.CreateLogger<IApplication>();

        // Get the service collection and service provider instances.
        this.ContainerBuilder = this.GetServices();

        // Initialize the graphics device manager.
        this.GraphicsDeviceManager = new GraphicsDeviceManager(this);

        // Configure the content manager.
        this.Content.RootDirectory = "Content";

        // Configure the game window.
        this.IsMouseVisible = true;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <inheritdoc />
    protected override void Initialize()
    {
        // Registe the graphics device manager.
        _ = this.ContainerBuilder.RegisterInstance<GraphicsDeviceManager>(this.GraphicsDeviceManager)
            .Keyed<GraphicsDeviceManager>("GraphicsDeviceManager")
            .As<GraphicsDeviceManager>()
            .SingleInstance();

        // Register the graphics device.
        _ = this.ContainerBuilder.RegisterInstance<GraphicsDevice>(this.GraphicsDevice)
            .Keyed<GraphicsDevice>("GraphicsDevice")
            .As<GraphicsDevice>()
            .SingleInstance();

        // Register the content manager.
        _ = this.ContainerBuilder.RegisterInstance<ContentManager>(this.Content)
            .Keyed<ContentManager>("ContentManager")
            .As<ContentManager>()
            .SingleInstance();

        // Call the base method.
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Create the sprite batch.
        this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

        // Register the sprite batch.
        _ = this.ContainerBuilder.RegisterInstance<SpriteBatch>(this.SpriteBatch)
            .Keyed<SpriteBatch>("SpriteBatch")
            .As<SpriteBatch>()
            .SingleInstance();

        // Call the base method.
        base.LoadContent();
    }

    /// <inheritdoc />
    protected override void BeginRun()
    {
        // Get the service provider.
        this.Provider = this.GetServiceProvider();

        // Get the server network socket.
        this.Socket = this.Provider.GetRequiredService<ClientSocket>();

        // Call the base method.
        base.BeginRun();
    }

    /// <inheritdoc />
    protected override void Update(GameTime gameTime)
    {
#if !IOS
        // Check to see if the back button or the escape key was pressed.
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            // Exit the application.
            this.Exit();
        }
#endif

        // Check to see if the start button or the enter key was pressed.
        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            // Check to see if the socket is running.
            if (!this.Socket.IsRunning)
            {
                // Start the socket.
                this.Socket.Start();

                // Attempt to connect to the server using async method.
                _ = this.ConnectToServerAsync();
            }
        }

        // Check to see if the big button or the back key was pressed.
        if (GamePad.GetState(PlayerIndex.One).Buttons.BigButton == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back))
        {
            // Check to see if the socket is running.
            if (this.Socket.IsRunning)
            {
                // Stop the socket.
                this.Socket.Stop();
            }
        }

        // Update the socket.
        this.Socket.Update();

        // Increment the accumulator.
        this.Accumulator += gameTime.ElapsedGameTime;

        // Call the variable update function.
        this.VariableUpdate(gameTime);

        // Iterate the fixed update function if the accumulator is greater than or equal to the fixed step.
        while (this.Accumulator >= this.FixedStep && this.Steps < this.MaxSteps)
        {
            // Calculate the fixed game time.
            GameTime fixedGameTime = new(gameTime.TotalGameTime, this.FixedStep);

            // Call the fixed update function.
            this.FixedUpdate(fixedGameTime);

            // Decrement the accumulator and increment the step counter.
            this.Accumulator -= this.FixedStep;
            this.Steps++;
        }

        // Reset the step counter.
        this.Steps = 0;

        // Call the base method.
        base.Update(gameTime);
    }

    /// <summary>
    /// Called on a variable timescale withing the update function.
    /// </summary>
    /// <param name="gameTime">The elapsed time since the last call to <see cref="FixedUpdate(GameTime)"/>.</param>
    protected virtual void VariableUpdate(GameTime gameTime)
    {

    }

    /// <summary>
    /// Called on a fixed timescale withing the update function.
    /// </summary>
    /// <param name="gameTime">The elapsed time since the last call to <see cref="FixedUpdate(GameTime)"/>.</param>
    protected virtual void FixedUpdate(GameTime gameTime)
    {

    }

    /// <summary>
    /// Asynchronously connects to the server using configuration values.
    /// </summary>
    private async Task ConnectToServerAsync()
    {
        try
        {
            this.Logger.LogInformation("Attempting to connect to server...");
            
            bool connected = await this.Socket.ConnectAsync();
            
            if (connected)
            {
                this.Logger.LogInformation("Successfully connected to the server.");
            }
            else
            {
                this.Logger.LogError("Unable to connect to the server, check that it is running.");
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred while connecting to the server.");
        }
    }

    /// <inheritdoc />
    protected override void Draw(GameTime gameTime)
    {
        // Clear the screen.
        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        // Call the base method.
        base.Draw(gameTime);
    }
}
