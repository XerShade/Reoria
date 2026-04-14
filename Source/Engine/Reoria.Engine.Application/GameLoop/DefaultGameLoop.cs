using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;

namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// A default implementation of a game loop with fixed and variable update support.
/// </summary>
public class DefaultGameLoop : IGameLoop
{
    private readonly ILogger<DefaultGameLoop> logger;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly object lockObject = new();
    private volatile bool isRunning = false;
    private long tickNumber = 0;
    private TimeSpan totalElapsedTime = TimeSpan.Zero;
    private TimeSpan accumulator = TimeSpan.Zero;
    private TimeSpan fixedStep = TimeSpan.FromSeconds(1.0 / 30.0);
    private int maxFixedSteps = 5;
    private TimeSpan previousTime = TimeSpan.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGameLoop"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="phases">The collection of game loop phases.</param>
    public DefaultGameLoop(ILogger<DefaultGameLoop> logger, IEnumerable<IGameLoopPhase> phases)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Phases = phases?.OrderByDescending(p => p.Priority).ToList() ?? throw new ArgumentNullException(nameof(phases));
        this.logger.LogDebug("Game loop initialized with {PhaseCount} phases", this.Phases.Count);
    }

    /// <inheritdoc />
    public bool IsRunning => this.isRunning;

    /// <inheritdoc />
    public IList<IGameLoopPhase> Phases { get; }

    /// <inheritdoc />
    public void Start()
    {
        lock (this.lockObject)
        {
            if (this.isRunning)
            {
                this.logger.LogWarning("Game loop is already running");
                return;
            }

            this.isRunning = true;
            this.tickNumber = 0;
            this.totalElapsedTime = TimeSpan.Zero;
            this.accumulator = TimeSpan.Zero;
            this.previousTime = TimeSpan.Zero;

            this.logger.LogInformation("Game loop started");
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        lock (this.lockObject)
        {
            if (!this.isRunning)
            {
                this.logger.LogWarning("Game loop is not running");
                return;
            }

            this.isRunning = false;
            this.cancellationTokenSource.Cancel();

            this.logger.LogInformation("Game loop stopped after {TickCount} ticks", this.tickNumber);
        }
    }

    /// <inheritdoc />
    public void Tick(GameTime gameTime)
    {
        if (!this.isRunning)
        {
            return;
        }

        try
        {
            // Update timing
            this.UpdateTiming(gameTime);

            // Create context for variable update
            var variableContext = new DefaultGameLoopContext(
                gameTime,
                this.tickNumber++,
                this.totalElapsedTime,
                isFixedUpdate: false,
                this.fixedStep,
                0);

            // Execute variable update phases
            this.ExecutePhasesAsync(variableContext, this.cancellationTokenSource.Token).GetAwaiter().GetResult();

            // Process fixed updates
            this.ProcessFixedUpdates(gameTime);
        }
        catch (OperationCanceledException)
        {
            this.logger.LogDebug("Game loop tick was cancelled");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred during game loop tick");
        }
    }

    private void UpdateTiming(GameTime gameTime)
    {
        this.totalElapsedTime += gameTime.ElapsedGameTime;
        this.accumulator += gameTime.ElapsedGameTime;
    }

    private void ProcessFixedUpdates(GameTime gameTime)
    {
        int fixedStepCount = 0;

        while (this.accumulator >= this.fixedStep && fixedStepCount < this.maxFixedSteps)
        {
            // Create context for fixed update
            var fixedContext = new DefaultGameLoopContext(
                gameTime,
                this.tickNumber++,
                this.totalElapsedTime,
                isFixedUpdate: true,
                this.fixedStep,
                fixedStepCount + 1);

            // Execute fixed update phases
            this.ExecutePhasesAsync(fixedContext, this.cancellationTokenSource.Token).GetAwaiter().GetResult();

            this.accumulator -= this.fixedStep;
            fixedStepCount++;
        }
    }

    private async Task ExecutePhasesAsync(IGameLoopContext context, CancellationToken cancellationToken)
    {
        var enabledPhases = this.Phases.Where(p => p.IsEnabled).ToList();

        if (enabledPhases.Count == 0)
        {
            this.logger.LogTrace("No enabled phases to execute for tick {TickNumber}", context.TickNumber);
            return;
        }

        this.logger.LogTrace("Executing {PhaseCount} phases for tick {TickNumber}", enabledPhases.Count, context.TickNumber);

        foreach (var phase in enabledPhases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Check if phase supports async execution
                if (phase.IsAsync)
                {
                    // Phase has async override - use ExecuteAsync
                    this.logger.LogTrace("Executing phase {PhaseName} asynchronously", phase.Name);
                    await phase.ExecuteAsync(context, cancellationToken);
                }
                else
                {
                    // Phase uses synchronous Execute - call directly
                    this.logger.LogTrace("Executing phase {PhaseName} synchronously", phase.Name);
                    phase.Execute(context, cancellationToken).GetAwaiter().GetResult();
                }
                
                stopwatch.Stop();

                this.logger.LogTrace("Phase {PhaseName} executed in {ElapsedMilliseconds}ms", 
                    phase.Name, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error executing phase {PhaseName}", phase.Name);
                throw;
            }
        }
    }

    /// <summary>
    /// Disposes the game loop resources.
    /// </summary>
    public void Dispose()
    {
        this.Stop();
        this.cancellationTokenSource.Dispose();
    }
}
