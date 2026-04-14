using Microsoft.Extensions.Logging;
using Autofac;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// A registry service that manages the discovery and registration of game loop phases.
/// </summary>
public class GameLoopPhaseRegistry : IGameLoopPhaseRegistry
{
    private readonly ILogger<GameLoopPhaseRegistry> logger;
    private readonly IComponentContext componentContext;
    private readonly Lazy<IEnumerable<IGameLoopPhase>> lazyPhases;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameLoopPhaseRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="componentContext">The component context for resolving phases.</param>
    public GameLoopPhaseRegistry(ILogger<GameLoopPhaseRegistry> logger, IComponentContext componentContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.componentContext = componentContext ?? throw new ArgumentNullException(nameof(componentContext));
        this.lazyPhases = new Lazy<IEnumerable<IGameLoopPhase>>(this.DiscoverPhases);
    }

    /// <inheritdoc />
    public IEnumerable<IGameLoopPhase> GetPhases()
    {
        return this.lazyPhases.Value;
    }

    /// <inheritdoc />
    public IEnumerable<IGameLoopPhase> GetEnabledPhases()
    {
        return this.GetPhases().Where(p => p.IsEnabled);
    }

    /// <inheritdoc />
    public IEnumerable<IGameLoopPhase> GetPhasesByPriority()
    {
        return this.GetEnabledPhases().OrderByDescending(p => p.Priority);
    }

    private IEnumerable<IGameLoopPhase> DiscoverPhases()
    {
        try
        {
            this.logger.LogDebug("Discovering game loop phases from DI container...");

            // Resolve all IGameLoopPhase implementations from the DI container
            var phases = this.componentContext.Resolve<IEnumerable<IGameLoopPhase>>().ToList();

            this.logger.LogInformation("Discovered {PhaseCount} game loop phases", phases.Count);

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var phase in phases)
                {
                    this.logger.LogDebug("Phase: {PhaseName} (Priority: {Priority}, Enabled: {Enabled})",
                        phase.Name, phase.Priority, phase.IsEnabled);
                }
            }

            return phases;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while discovering game loop phases");
            throw;
        }
    }
}
