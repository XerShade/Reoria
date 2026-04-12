using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Network.Sockets;
using System.Diagnostics;

namespace Reoria.Server.Application;

/// <summary>
/// Defines the server application and its functionality.
/// </summary>
public class ServerApplication : IApplication
{
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

    public ServerApplication(ILogger<IApplication> logger, [KeyFilter("CommandLineArgs")] string[] args)
    {
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
        // Start the stopwatch to measure the application time.
        this.Timer.Start();
        this.PreviousTime = this.Timer.Elapsed;

        // Start the network socket.
        this.Socket.Start();

        // Start the update loop.
        while (this.Running)
        {
            // Update the network socket.
            this.Socket.Update();

            // Calculate the elapsed time since the last update.
            TimeSpan now = this.Timer.Elapsed;
            TimeSpan frameTime = now - this.PreviousTime;
            this.PreviousTime = now;

            // Increment the accumulator.
            this.Accumulator += frameTime;

            // Calculate the game time for the current update cycle.
            GameTime gameTime = new(now, frameTime);

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

            // Pause thread execution to reduce CPU usage.
            Thread.Sleep(1);
        }

        // Stop the network socket.
        this.Socket.Stop();
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

    /// <inheritdoc />
    public virtual void Exit()
        => this.Running = false;

    /// <inheritdoc />
    public virtual void Dispose()
        => GC.SuppressFinalize(this);
}