using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.GameLoop;
using Reoria.Engine.Application.GameLoop.Phases;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Network.Sockets;
using System.Diagnostics;

namespace Reoria.Server.Core.Application;

/// <summary>
/// Defines the server application and its functionality.
/// </summary>
public class ServerApplication : IApplication
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
    public virtual IServiceProvider Provider { get; init; }

    /// <summary>
    /// Constructs a new instance of <see cref="ServerApplication"/>.
    /// </summary>
    /// <param name="logger">A logger instance to log messages to.</param>
    /// <param name="args">The command line arguments.</param>
    /// <param name="context">The application boot context.</param>
    public ServerApplication(ILogger<IApplication> logger, [KeyFilter("CommandLineArgs")] string[] args, AppBootContext context)
    {
        // Store the platform.
        this.Platform = context.Platform;

        // Store the logger and report the initialization.
        this.Logger = logger;
        this.Logger.LogInformation("Initializing server application...");

        // Start a new stopwatch to measure the application time.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Discover the application injectors.
        this.Injectors = this.DiscoverInjectors();

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.LoggerFactory.CreateLogger<IApplication>();

        // Get the service collection and service provider instances.
        this.ContainerBuilder = this.GetServices();
        this.Provider = this.GetServiceProvider();

        // Get the server network socket.
        this.Socket = this.Provider.GetRequiredService<ServerSocket>();

        // Initialize the game loop with phases.
        this.GameLoop = this.InitializeGameLoop();

        // Stop the stopwatch to measure the application time.
        stopwatch.Stop();
    }

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
    /// Gets an instance of <see cref="ServerSocket"/> to manage the networking functionality.
    /// </summary>
    protected ServerSocket Socket { get; set; }
    /// <summary>
    /// Gets the game loop instance that manages the update cycle.
    /// </summary>
    protected IGameLoop GameLoop { get; set; }

    /// <inheritdoc />
    public virtual void Run()
    {
        this.Logger.LogInformation("Starting server application main loop...");

        try
        {
            // Start the application components.
            this.StartApplication();

            // Start the game loop.
            this.GameLoop.Start();

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

                // Process a single tick through the game loop.
                this.GameLoop.Tick(gameTime);

                // Pause thread execution to reduce CPU usage.
                Thread.Sleep(1);
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
    /// Initializes the game loop using the DI-based factory pattern.
    /// </summary>
    /// <returns>A configured game loop instance.</returns>
    protected virtual IGameLoop InitializeGameLoop()
    {
        this.Logger.LogDebug("Initializing server game loop using DI factory...");

        // Get the game loop factory from DI container
        var gameLoopFactory = this.Provider.GetRequiredService<IGameLoopFactory>();

        // Get any custom phases from injectors that implement IGameLoopPhase
        List<IGameLoopPhase> injectorPhases = [.. this.Injectors.OfType<IGameLoopPhase>()];

        // Create the game loop with auto-discovered phases plus any injector phases
        IGameLoop gameLoop = injectorPhases.Count > 0 
            ? gameLoopFactory.CreateGameLoop(injectorPhases)
            : gameLoopFactory.CreateGameLoop();

        if (injectorPhases.Count > 0 && this.Logger.IsEnabled(LogLevel.Debug))
        {
            this.Logger.LogDebug("Added {Count} injector phases to server game loop", injectorPhases.Count);
        }

        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation("Server game loop initialized with {PhaseCount} phases", gameLoop.Phases.Count);
        }

        return gameLoop;
    }

    /// <summary>
    /// Starts the application components.
    /// </summary>
    protected virtual void StartApplication()
    {
        this.Logger.LogDebug("Starting application components...");

        // Start the network socket.
        this.Socket.Start();

        // Notify application lifecycle injectors that the application is starting.
        List<IApplicationLifecycleInjector> lifecycleInjectors = [.. this.Injectors.OfType<IApplicationLifecycleInjector>()];
        foreach (IApplicationLifecycleInjector? injector in lifecycleInjectors)
        {
            try
            {
                injector.OnApplicationStart();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Application lifecycle injector {InjectorType} failed during application start", injector.GetType().Name);
            }
        }

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
            // Stop the game loop.
            this.GameLoop?.Stop();

            // Stop the network socket.
            this.Socket?.Stop();

            // Notify application lifecycle injectors that the application is stopping.
            List<IApplicationLifecycleInjector> lifecycleInjectors = [.. this.Injectors.OfType<IApplicationLifecycleInjector>()];
            foreach (IApplicationLifecycleInjector? injector in lifecycleInjectors)
            {
                try
                {
                    injector.OnApplicationStop();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Application lifecycle injector {InjectorType} failed during application stop", injector.GetType().Name);
                }
            }

            this.Logger.LogInformation("Application components stopped successfully");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error occurred while stopping application components");
        }
    }

    /// <summary>
    /// Called on a variable timescale within the update function.
    /// Override this method to add custom variable update logic.
    /// </summary>
    /// <param name="gameTime">The elapsed time since the last call to <see cref="FixedUpdate(GameTime)"/>.</param>
    protected virtual void VariableUpdate(GameTime gameTime)
    {
        // Custom variable update logic can be added here
        // Consider creating a custom IGameLoopPhase instead for better modularity
    }

    /// <summary>
    /// Called on a fixed timescale within the update function.
    /// Override this method to add custom fixed update logic.
    /// </summary>
    /// <param name="gameTime">The elapsed time since the last call to <see cref="FixedUpdate(GameTime)"/>.</param>
    protected virtual void FixedUpdate(GameTime gameTime)
    {
        // Custom fixed update logic can be added here
        // Consider creating a custom IGameLoopPhase instead for better modularity
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
            this.GameLoop?.Dispose();
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