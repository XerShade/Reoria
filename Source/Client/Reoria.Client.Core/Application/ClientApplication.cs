using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Client.Core.Injectors;
using Reoria.Client.Core.Services;
using Reoria.Client.Network.Sockets;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Application.Services;
using Reoria.Engine.Application.Services.Interfaces;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using GameBase = Microsoft.Xna.Framework.Game;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Reoria.Client.Core.Application;

/// <summary>
/// Defines the client application and its functionality.
/// </summary>
public class ClientApplication : GameBase, IApplication, IDisposable
{
    /// <inheritdoc />
    public virtual Platform Platform { get; init; }
    /// <inheritdoc />
    public virtual ILogger<IApplication> Logger { get; init; }
    /// <inheritdoc />
    public virtual IPhaseService PhaseService { get; init; }
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
    /// Gets the accumulator for the fixed update loop.
    /// </summary>
    protected TimeSpan Accumulator { get; set; }
    /// <summary>
    /// Gets the fixed step for the fixed update loop.
    /// </summary>
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 60.0);
    /// <summary>
    /// Gets the maximum steps for the fixed update loop allowed per update cycle.
    /// </summary>
    protected int MaxSteps { get; init; } = 5;
    /// <summary>
    /// Gets the current number of steps for the fixed update loop.
    /// </summary>
    protected int Steps { get; set; } = 0;
    /// <summary>
    /// Gets the target frame rate for frame rate limiting.
    /// </summary>
    protected int TargetFrameRate { get; init; } = 60;
    /// <summary>
    /// Gets the minimum frame time for frame rate limiting.
    /// </summary>
    protected TimeSpan MinFrameTime { get; init; } = TimeSpan.FromSeconds(1.0 / 60.0);
    /// <summary>
    /// Gets the previous total game time for delta time calculation.
    /// </summary>
    protected TimeSpan PreviousTotalGameTime { get; set; }
    /// <summary>
    /// Gets the smoothed delta time for stable updates.
    /// </summary>
    protected TimeSpan SmoothedDeltaTime { get; set; }
    /// <summary>
    /// Gets the smoothing factor for delta time (0-1, higher = more smoothing).
    /// </summary>
    protected float DeltaTimeSmoothingFactor { get; init; } = 0.9f;
    /// <summary>
    /// Gets an instance of <see cref="ClientSocket"/> to manage the networking functionality.
    /// </summary>
    protected ClientSocket Socket { get; set; }
    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Constructs a new instance of <see cref="ClientApplication"/>.
    /// </summary>
    /// <param name="logger">A logger instance to log messages to.</param>
    /// <param name="context">The application boot context.</param>
    public ClientApplication(ILogger<IApplication> logger, AppBootContext context)
    {
        // Store the platform.
        this.Platform = context.Platform;

        // Store the logger and report the initialization.
        this.Logger = logger;
        this.Logger.LogInformation("Initializing client application...");

        // Store the phase service.
        this.PhaseService = new PhaseService().AddAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(context.Args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.LoggerFactory.CreateLogger<IApplication>();

        // Get the service collection and service provider instances.
        this.ContainerBuilder = this.GetServices();

        // Initialize the graphics device manager.
        this.GraphicsDeviceManager = new(this);

        // Configure the content manager.
        this.Content.RootDirectory = "Assets";

        // Configure the game window.
        this.IsMouseVisible = true;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <inheritdoc />
    protected override void Initialize()
    {
        // Execute the game loop phase participants.
        this.PhaseService.ExecutePhase<IGameInitializeGraphics>(
            phase => phase.OnInitializeGraphics(this.ContainerBuilder, this.GraphicsDeviceManager, this.GraphicsDevice));

        // Call the base method.
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Execute the game loop phase participants.
        this.PhaseService.ExecutePhase<IGameLoadContent>(
            phase => phase.OnLoadContent(this.ContainerBuilder, this.Content));

        // Call the base method.
        base.LoadContent();

        // Check to see if the platform is Android or iOS.
        if (this.Platform is Platform.Android or Platform.iOS)
        {
            // Finalize the initialization.
            this.FinalizeInitialization();
        }
    }

    /// <inheritdoc />
    protected override void BeginRun()
    {
        // Check to see if the platform is Windows or Desktop.
        if (this.Platform is Platform.Windows or Platform.Desktop)
        {
            // Finalize the initialization.
            this.FinalizeInitialization();
        }

        // Call the base method.
        base.BeginRun();
    }

    /// <summary>
    /// Finalizes the initialization of the application and acquires service instances.
    /// </summary>
    protected virtual void FinalizeInitialization()
    {
        // Temporary: Add services to the container.
        _ = this.ContainerBuilder.RegisterType<GraphicsDeviceService>().AsImplementedInterfaces().SingleInstance();
        _ = this.ContainerBuilder.RegisterType<ContentManagerService>().AsImplementedInterfaces().SingleInstance();
        _ = this.ContainerBuilder.RegisterType<SpriteBatchService>().AsImplementedInterfaces().SingleInstance();

        // Initialize the dependency injection provider.
        this.InitializeProvider();

        // Get the server network socket.
        this.Socket = this.Provider.GetRequiredService<ClientSocket>();

        // Execute the game loop phase participants.
        this.PhaseService.ExecutePhase<IGameInitialize>(
            phase => phase.OnInitializeGame(this));

        // Notify application phase participants that the application is starting.
        this.PhaseService.ExecutePhase<IApplicationStart>(phase => phase.OnApplicationStart());
    }

    /// <summary>
    /// Initializes the dependcy injection provider and initializes the game loop.
    /// </summary>
    protected virtual void InitializeProvider()
    {        
        // Get the service provider.
        this.Provider = this.GetServiceProvider();

        // Set the service provider on the phase service so future phase participant resolutions use DI.
        // This is done after the container is built so phase participants that run after bootstrap
        // (e.g., game loop phase participants) can be resolved via DI with their dependencies.
        _ = this.PhaseService.SetServiceProvider(this.Provider);
    }

    /// <inheritdoc />
    protected override void Update(GameTime gameTime)
    {
        // Update the network socket first, as it may have data to process that affects the game state.
        this.Socket.Update();

        // Update user input second, as lagging input may affect the game state and player happiness.
        this.PhaseService.ExecutePhase<IGameInput>(phase => phase.OnHandleInput(gameTime, Keyboard.GetState(), Mouse.GetState()));

        // Apply frame rate limiting
        this.ApplyFrameRateLimiting(gameTime);

        // Calculate smoothed delta time
        this.CalculateSmoothedDeltaTime(gameTime);

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

        // Process a single tick through the game loop with smoothed delta time.
        GameTime smoothedGameTime = new(gameTime.TotalGameTime, this.SmoothedDeltaTime);

        // Process the game loop.
        this.HandleVariableUpdate(smoothedGameTime);

        // Update the previous game time for next frame.
        this.PreviousTotalGameTime = gameTime.TotalGameTime;

        // Call the base method.
        base.Update(gameTime);
    }

    /// <summary>
    /// Handles variable updates for the game loop.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleVariableUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameVariableUpdate>(phase => phase.OnVariableUpdate(gameTime));

    /// <summary>
    /// Handles fixed updates for the game loop.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleFixedUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameFixedUpdate>(phase => phase.OnFixedUpdate(gameTime));

    /// <summary>
    /// Applies frame rate limiting to prevent excessive CPU usage.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void ApplyFrameRateLimiting(GameTime gameTime)
    {
        // Calculate the time since the last frame
        TimeSpan currentFrameTime = gameTime.TotalGameTime - this.PreviousTotalGameTime;

        // If the frame was too fast, wait to maintain the target frame rate
        if (currentFrameTime < this.MinFrameTime)
        {
            TimeSpan waitTime = this.MinFrameTime - currentFrameTime;
            System.Threading.Thread.Sleep(waitTime);
        }
    }

    /// <summary>
    /// Calculates smoothed delta time to reduce frame time jitter.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void CalculateSmoothedDeltaTime(GameTime gameTime)
    {
        // Initialize smoothed delta time on the first frame
        if (this.PreviousTotalGameTime == TimeSpan.Zero)
        {
            this.SmoothedDeltaTime = gameTime.ElapsedGameTime;
            return;
        }

        // Calculate the current delta time
        TimeSpan currentDeltaTime = gameTime.ElapsedGameTime;

        // Apply exponential moving average smoothing
        double smoothedTicks = (this.DeltaTimeSmoothingFactor * this.SmoothedDeltaTime.Ticks) +
                              ((1.0 - this.DeltaTimeSmoothingFactor) * currentDeltaTime.Ticks);

        this.SmoothedDeltaTime = TimeSpan.FromTicks((long)smoothedTicks);
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
        // Handle rendering of all drawing injectors.
        this.HandleRendering(gameTime);

        // Call the base method
        base.Draw(gameTime);
    }

    /// <summary>
    /// Handles all rendering operations for the client.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleRendering(GameTime gameTime)
    {
        if (this.Provider == null)
        {
            return; // Not initialized yet
        }

        try
        {
            // Access the sprite batch, throwing an exception if it is null, I will make a better function later.
            SpriteBatch spriteBatch = this.Provider.GetRequiredService<SpriteBatch>();

            // Execute all rendering phase participants
            this.PhaseService.ExecutePhase<IGamePreRender>(phase => phase.OnPreRender(gameTime, this.GraphicsDevice, spriteBatch));
            this.PhaseService.ExecutePhase<IGameRender>(phase => phase.OnRender(gameTime, this.GraphicsDevice, spriteBatch));
            this.PhaseService.ExecutePhase<IGamePostRender>(phase => phase.OnPostRender(gameTime, this.GraphicsDevice, spriteBatch));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error during rendering");
        }
    }

    /// <summary>
    /// Releases all resources used by ClientApplication.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!this.IsDisposed && disposing)
        {
            try
            {
                // Notify application phase participants that the application is stopping
                this.PhaseService.ExecutePhase<IApplicationStop>(phase => phase.OnApplicationStop());

                // Dispose the socket if it exists
                this.Socket?.Dispose();

                // Dispose the graphics device manager if it exists
                this.GraphicsDeviceManager?.Dispose();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error occurred during client application disposal");
            }
            finally
            {
                this.IsDisposed = true;
            }
        }

        // Call the base dispose method
        base.Dispose(disposing);
    }
}