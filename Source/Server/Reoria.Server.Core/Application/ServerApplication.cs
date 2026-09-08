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

        // Start a new stopwatch to measure the application time.
        Stopwatch stopwatch = Stopwatch.StartNew();

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

                // Update the network socket first, as it may have data to process that affects the game state.
                this.Socket.Update();

                // Update the game loop.
                this.HandleVariableUpdate(gameTime);

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