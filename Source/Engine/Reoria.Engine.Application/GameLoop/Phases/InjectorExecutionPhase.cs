using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Engine.Application.GameLoop.Phases;

/// <summary>
/// A game loop phase that executes application injectors.
/// </summary>
public class InjectorExecutionPhase : IGameLoopPhase
{
    private readonly ILogger<InjectorExecutionPhase> logger;
    private readonly IApplication application;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectorExecutionPhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="application">The application instance containing injectors.</param>
    public InjectorExecutionPhase(ILogger<InjectorExecutionPhase> logger, IApplication application)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.application = application ?? throw new ArgumentNullException(nameof(application));
    }

    /// <inheritdoc />
    public string Name => "Injector Execution";

    /// <inheritdoc />
    public int Priority => 50;

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        if (this.application.Injectors.Count == 0)
        {
            this.logger.LogTrace("No injectors to execute for tick {TickNumber}", context.TickNumber);
            return;
        }

        this.logger.LogTrace("Executing {InjectorCount} injectors for tick {TickNumber}", 
            this.application.Injectors.Count, context.TickNumber);

        // Execute variable update injectors
        if (!context.IsFixedUpdate)
        {
            var variableUpdateInjectors = this.application.Injectors.OfType<IVariableUpdateInjector>().ToList();
            
            if (variableUpdateInjectors.Count > 0)
            {
                this.logger.LogTrace("Executing {InjectorCount} variable update injectors for tick {TickNumber}", 
                    variableUpdateInjectors.Count, context.TickNumber);

                foreach (var injector in variableUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        
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
            // Execute fixed update injectors
            var fixedUpdateInjectors = this.application.Injectors.OfType<IFixedUpdateInjector>().ToList();
            
            if (fixedUpdateInjectors.Count > 0)
            {
                this.logger.LogTrace("Executing {InjectorCount} fixed update injectors for tick {TickNumber}", 
                    fixedUpdateInjectors.Count, context.TickNumber);

                foreach (var injector in fixedUpdateInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        
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
}
