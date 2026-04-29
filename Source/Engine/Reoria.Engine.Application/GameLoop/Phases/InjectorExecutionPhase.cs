using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;
using Reoria.Engine.Application.Injectors;
using System.Diagnostics;

namespace Reoria.Engine.Application.GameLoop.Phases;

/// <summary>
/// A game loop phase that executes application injectors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InjectorExecutionPhase"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="variableUpdateInjectors">The variable update injectors to execute.</param>
/// <param name="fixedUpdateInjectors">The fixed update injectors to execute.</param>
public class InjectorExecutionPhase(ILogger<InjectorExecutionPhase> logger,
    IEnumerable<IVariableUpdateInjector> variableUpdateInjectors, IEnumerable<IFixedUpdateInjector> fixedUpdateInjectors) : IGameLoopPhase
{
    private readonly ILogger<InjectorExecutionPhase> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEnumerable<IVariableUpdateInjector> variableUpdateInjectors = variableUpdateInjectors ?? throw new ArgumentNullException(nameof(variableUpdateInjectors));
    private readonly IEnumerable<IFixedUpdateInjector> fixedUpdateInjectors = fixedUpdateInjectors ?? throw new ArgumentNullException(nameof(fixedUpdateInjectors));

    /// <inheritdoc />
    public string Name => "Injector Execution";

    /// <inheritdoc />
    public int Priority => 90; // Execute after NetworkUpdate (100) but before rendering phases

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsAsync => true; // Supports both sync and async execution

    /// <inheritdoc />
    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        if (!this.variableUpdateInjectors.Any() && !this.fixedUpdateInjectors.Any())
        {
            this.logger.LogTrace("No injectors to execute for tick {TickNumber}", context.TickNumber);
            return;
        }

        this.logger.LogTrace("Executing {InjectorCount} injectors for tick {TickNumber}",
            this.variableUpdateInjectors.Count() + this.fixedUpdateInjectors.Count(), context.TickNumber);

        // Execute variable update injectors
        if (!context.IsFixedUpdate)
        {
            if (this.variableUpdateInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} variable update injectors for tick {TickNumber}",
                    this.variableUpdateInjectors.Count(), context.TickNumber);

                foreach (IVariableUpdateInjector injector in this.variableUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        injector.OnVariableUpdate(context.GameTime);

                        stopwatch.Stop();

                        this.logger.LogTrace("Variable update injector {InjectorType} executed in {ElapsedMilliseconds}ms",
                            injector.GetType().Name, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing variable update injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }
        }
        else
        {
            if (this.fixedUpdateInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} fixed update injectors for tick {TickNumber}",
                    this.fixedUpdateInjectors.Count(), context.TickNumber);

                foreach (IFixedUpdateInjector injector in this.fixedUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        injector.OnFixedUpdate(context.GameTime);

                        stopwatch.Stop();

                        this.logger.LogTrace("Fixed update injector {InjectorType} executed in {ElapsedMilliseconds}ms",
                            injector.GetType().Name, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing fixed update injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Execute(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        if (!this.variableUpdateInjectors.Any() && !this.fixedUpdateInjectors.Any())
        {
            this.logger.LogTrace("No injectors to execute for tick {TickNumber}", context.TickNumber);
            return Task.CompletedTask;
        }

        this.logger.LogTrace("Executing {InjectorCount} injectors for tick {TickNumber}",
            this.variableUpdateInjectors.Count() + this.fixedUpdateInjectors.Count(), context.TickNumber);

        // Execute variable update injectors
        if (!context.IsFixedUpdate)
        {
            if (this.variableUpdateInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} variable update injectors for tick {TickNumber}",
                    this.variableUpdateInjectors.Count(), context.TickNumber);

                foreach (IVariableUpdateInjector injector in this.variableUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        injector.OnVariableUpdate(context.GameTime);

                        stopwatch.Stop();

                        this.logger.LogTrace("Variable update injector {InjectorType} executed in {ElapsedMilliseconds}ms",
                            injector.GetType().Name, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing variable update injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }
        }
        else
        {
            if (this.fixedUpdateInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} fixed update injectors for tick {TickNumber}",
                    this.fixedUpdateInjectors.Count(), context.TickNumber);

                foreach (IFixedUpdateInjector injector in this.fixedUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        injector.OnFixedUpdate(context.GameTime);

                        stopwatch.Stop();

                        this.logger.LogTrace("Fixed update injector {InjectorType} executed in {ElapsedMilliseconds}ms",
                            injector.GetType().Name, stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing fixed update injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}