using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Application.Services;
using Reoria.Engine.Application.Services.Interfaces;
using Reoria.Server.Network.Sockets;
using System.Diagnostics;

namespace Reoria.Server.Core.Application;

/// <summary>
/// Defines the server application and its functionality.
/// Implements a game loop similar to Unity's: FixedUpdate → Update → LateUpdate → Render
/// </summary>
public class ServerApplication : IApplication
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
    public virtual IServiceProvider Provider { get; init; }

    /// <summary>
    /// Gets a value indicating whether the application is running.
    /// </summary>
    protected virtual bool Running { get; set; } = true;
    /// <summary>
    /// Gets the stopwatch to measure the application time.
    /// </summary>
    protected virtual Stopwatch Timer { get; init; } = new();
    /// <summary>
    /// Gets the previous time for the update loop.
    /// </summary>
    protected virtual TimeSpan PreviousTime { get; set; }
    /// <summary>
    /// Gets the accumulator for the fixed update loop.
    /// </summary>
    protected TimeSpan Accumulator { get; set; }
    /// <summary>
    /// Gets the fixed step for the fixed update loop (30 FPS default for physics).
    /// </summary>
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 30.0);
    /// <summary>
    /// Gets the maximum fixed steps allowed per update cycle to prevent spiral of death.
    /// </summary>
    protected int MaxFixedSteps { get; init; } = 5;
    /// <summary>
    /// Gets the current number of fixed steps processed.
    /// </summary>
    protected int CurrentFixedSteps { get; set; }
    /// <summary>
    /// Gets an instance of <see cref="ServerSocket"/> to manage the networking functionality.
    /// </summary>
    protected ServerSocket Socket { get; set; }

    /// <summary>
    /// Constructs a new instance of <see cref="ServerApplication"/>.
    /// </summary>
    /// <param name="logger">A logger instance to log messages to.</param>
    /// <param name="context">The application boot context.</param>
    public ServerApplication(ILogger<IApplication> logger, AppBootContext context)
    {
        // Store the platform.
        this.Platform = context.Platform;

        // Store the logger and report the initialization.
        this.Logger = logger;
        this.Logger.LogInformation("Initializing server application...");

        // Store the phase service.
        this.PhaseService = new PhaseService().AddAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(context.Args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.LoggerFactory.CreateLogger<IApplication>();

        // Get the service collection and service provider instances.
        this.ContainerBuilder = this.GetServices();
        this.Provider = this.GetServiceProvider();

        // Set the service provider on the phase service so future phase participant resolutions use DI.
        // This is done after the container is built so phase participants that run after bootstrap
        // (e.g., game loop phase participants) can be resolved via DI with their dependencies.
        _ = this.PhaseService.SetServiceProvider(this.Provider);

        // Get the server network socket.
        this.Socket = this.Provider.GetRequiredService<ServerSocket>();

        this.Logger.LogInformation("Server application initialized successfully.");
    }

    /// <inheritdoc />
    public virtual void Run()
    {
        this.Logger.LogInformation("Starting server application main loop...");

        try
        {
            // Start the application components.
            this.StartApplication();

            // Start the stopwatch to measure the application time.
            this.Timer.Start();
            this.PreviousTime = this.Timer.Elapsed;

            // Main application loop.
            while (this.Running)
            {
                // Calculate the elapsed time since the last update.
                TimeSpan now = this.Timer.Elapsed;
                TimeSpan frameTime = now - this.PreviousTime;
                this.PreviousTime = now;

                // Create game time for the current update cycle.
                GameTime gameTime = new(now, frameTime);

                // 1. Update the network socket first, as it may have data to process that affects the game state.
                this.Socket.Update();

                // 2. Handle variable updates (runs once per frame)
                this.HandleVariableUpdate(gameTime);

                // 3. Handle fixed updates (runs at fixed intervals - physics, game logic)
                this.HandleFixedUpdates(gameTime);

                // 4. Late update - post-processing after all game logic
                this.HandleLateUpdate(gameTime);

                // 5. Frame timing control - prevent CPU spinning
                this.ControlFrameTiming(frameTime);
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "An error occurred in the main application loop");
            throw;
        }
        finally
        {
            // Ensure cleanup happens even if an exception occurs.
            this.StopApplication();
        }

        this.Logger.LogInformation("Server application main loop ended");
    }

    /// <summary>
    /// Starts the application components.
    /// </summary>
    protected virtual void StartApplication()
    {
        this.Logger.LogDebug("Starting application components...");

        // Start the network socket.
        this.Socket.Start();

        // Notify application phase participants that the application is starting.
        this.PhaseService.ExecutePhase<IApplicationStart>(phase => phase.OnApplicationStart());

        this.Logger.LogInformation("Application components started successfully");
    }

    /// <summary>
    /// Stops the application components.
    /// </summary>
    protected virtual void StopApplication()
    {
        this.Logger.LogDebug("Stopping application components...");

        try
        {
            // Stop the network socket.
            this.Socket?.Stop();

            // Notify application phase participants that the application is stopping.
            this.PhaseService.ExecutePhase<IApplicationStop>(phase => phase.OnApplicationStop());

            this.Logger.LogInformation("Application components stopped successfully");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred while stopping application components");
        }
    }

    /// <summary>
    /// Handles variable updates that run once per frame.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleVariableUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameVariableUpdate>(phase => phase.OnVariableUpdate(gameTime));

    /// <summary>
    /// Handles fixed updates that run at a fixed interval (e.g., physics, game logic).
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
    /// Handles late update - runs after all Update logic, useful for post-processing, cleanup, etc.
    /// </summary>
    /// <param name="gameTime">The current game time.</param>
    protected virtual void HandleLateUpdate(GameTime gameTime)
        => this.PhaseService.ExecutePhase<IGameLateUpdate>(phase => phase.OnLateUpdate(gameTime));

    /// <summary>
    /// Controls frame timing to prevent excessive CPU usage while maintaining smooth gameplay.
    /// Uses a more sophisticated approach than simple Thread.Sleep.
    /// </summary>
    /// <param name="frameTime">The time taken for the current frame.</param>
    protected virtual void ControlFrameTiming(TimeSpan frameTime)
    {
        // Calculate sleep time based on target frame rate (30 FPS = ~33ms per frame)
        TimeSpan targetFrameTime = TimeSpan.FromTicks(this.FixedStep.Ticks);
        TimeSpan remainingTime = targetFrameTime - frameTime;

        // If we have remaining time, sleep briefly to reduce CPU usage
        if (remainingTime > TimeSpan.Zero)
        {
            // Only sleep if we have enough time to make it worthwhile
            if (remainingTime > TimeSpan.FromMilliseconds(1))
            {
                System.Threading.Thread.Sleep(remainingTime);
            }
            else
            {
                // For very short remaining times, use a busy spin wait
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (stopwatch.Elapsed < remainingTime) { }
            }
        }
    }

    /// <inheritdoc />
    public virtual void Exit()
        => this.Running = false;

    /// <inheritdoc />
    public virtual void Dispose()
    {
        try
        {
            this.StopApplication();
            this.Socket?.Dispose();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred during application disposal");
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}