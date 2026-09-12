using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Client.Core.Services;
using Reoria.Client.Network.Sockets;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Application.Services;
using Reoria.Engine.Application.Services.Interfaces;
using System.Diagnostics;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using GameBase = Microsoft.Xna.Framework.Game;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Reoria.Client.Core.Application;

/// <summary>
/// Defines the client application and its functionality.
/// Implements a game loop similar to Unity's: FixedUpdate → Update → LateUpdate → Render
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
    /// Gets the fixed step for the fixed update loop (60 FPS default).
    /// </summary>
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 60.0);
    /// <summary>
    /// Gets the maximum fixed steps allowed per update cycle to prevent spiral of death.
    /// </summary>
    protected int MaxFixedSteps { get; init; } = 5;
    /// <summary>
    /// Gets the current number of fixed steps processed.
    /// </summary>
    protected int CurrentFixedSteps { get; set; }
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

    /// <summary>
    /// Tracks whether we're in the fixed update phase.
    /// </summary>
    protected bool IsFixedUpdate { get; set; }

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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

    /// <inheritdoc />
    protected override void Initialize()
    {
        // Execute the game loop phase participants for graphics initialization.
        this.PhaseService.ExecutePhase<IGameInitializeGraphics>(
            phase => phase.OnInitializeGraphics(this.ContainerBuilder, this.GraphicsDeviceManager, this.GraphicsDevice));

        // Call the base method.
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Execute the game loop phase participants for content loading.
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

        // Execute the game loop phase participants for game initialization.
        this.PhaseService.ExecutePhase<IGameInitialize>(
            phase => phase.OnInitializeGame(this));

        // Notify application phase participants that the application is starting.
        this.PhaseService.ExecutePhase<IApplicationStart>(phase => phase.OnApplicationStart());
    }

    /// <inheritdoc />
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
        // 1. Process network socket updates first (as it may have data to process)
        this.Socket.Update();

        // 2. Handle user input
        this.PhaseService.ExecutePhase<IGameInput>(
            phase => phase.OnHandleInput(gameTime, Keyboard.GetState(), Mouse.GetState()));

        // 3. Apply frame rate limiting
        this.ApplyFrameRateLimiting(gameTime);

        // 4. Calculate smoothed delta time
        this.CalculateSmoothedDeltaTime(gameTime);

        // 5. Check for exit conditions
#if !IOS
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            this.Exit();
        }
#endif
        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            if (!this.Socket.IsRunning)
            {
                this.Socket.Start();
                _ = this.ConnectToServerAsync();
            }
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.BigButton == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back))
        {
            if (this.Socket.IsRunning)
            {
                this.Socket.Stop();
            }
        }

        // 6. Process variable update (runs once per frame)
        this.HandleVariableUpdate(gameTime);

        // 7. Process fixed updates (runs at fixed intervals)
        this.HandleFixedUpdates(gameTime);

        // 8. Late update - post-processing after all game logic
        this.HandleLateUpdate(gameTime);

        // 9. Update previous game time
        this.PreviousTotalGameTime = gameTime.TotalGameTime;

        // Call the base method (this triggers Draw)
        base.Update(gameTime);
    }

    /// <summary>
    /// Handles variable updates that run once per frame.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleVariableUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameVariableUpdate>(phase => phase.OnVariableUpdate(gameTime));

    /// <summary>
    /// Handles fixed updates that run at a fixed interval (e.g., physics).
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleFixedUpdates(GameTime gameTime)
    {
        // Accumulate time since last fixed update
        this.Accumulator += gameTime.ElapsedGameTime;

        // Process fixed steps while accumulator exceeds the fixed step
        while (this.Accumulator >= this.FixedStep && this.CurrentFixedSteps < this.MaxFixedSteps)
        {
            // Create a consistent game time for the fixed step
            GameTime fixedGameTime = new(
                this.Accumulator,
                this.FixedStep);

            // Process the fixed update
            this.HandleFixedUpdate(fixedGameTime);

            // Subtract the fixed step from the accumulator
            this.Accumulator -= this.FixedStep;
            this.CurrentFixedSteps++;
        }

        // Reset fixed steps counter if we've processed the maximum
        if (this.CurrentFixedSteps >= this.MaxFixedSteps)
        {
            this.Accumulator = TimeSpan.Zero;
            this.CurrentFixedSteps = 0;
        }
    }

    /// <summary>
    /// Handles a single fixed update step.
    /// </summary>
    /// <param name="gameTime">The game time for the fixed step.</param>
    protected virtual void HandleFixedUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameFixedUpdate>(phase => phase.OnFixedUpdate(gameTime));

    /// <summary>
    /// Handles late update - runs after all Update logic, useful for camera follow, UI updates, etc.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleLateUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameLateUpdate>(phase => phase.OnLateUpdate(gameTime));

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
            // Use busy wait for shorter durations, sleep for longer
            if (waitTime > TimeSpan.FromMilliseconds(1))
            {
                System.Threading.Thread.Sleep(waitTime);
            }
            else
            {
                // Brief busy spin for very short waits
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (stopwatch.Elapsed < waitTime) { }
            }
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
    /// Follows Unity's render pipeline: PreRender → Render → PostRender
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
            // Access the sprite batch, throwing an exception if it is null
            SpriteBatch spriteBatch = this.Provider.GetRequiredService<SpriteBatch>();

            // Execute rendering phase participants in Unity's order:
            // 1. OnPreRender - setup, culling, etc.
            // 2. OnRender - main rendering
            // 3. OnPostRender - effects, overlays, etc.
            this.PhaseService.ExecutePhase<IGamePreRender>(
                phase => phase.OnPreRender(gameTime, this.GraphicsDevice, spriteBatch));

            this.PhaseService.ExecutePhase<IGameRender>(
                phase => phase.OnRender(gameTime, this.GraphicsDevice, spriteBatch));

            this.PhaseService.ExecutePhase<IGamePostRender>(
                phase => phase.OnPostRender(gameTime, this.GraphicsDevice, spriteBatch));
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