using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.GameLoop.Factories.Interfaces;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;

namespace Reoria.Engine.Application.GameLoop.Factories;

/// <summary>
/// A factory service for creating game loops with properly injected phases.
/// </summary>
public class GameLoopFactory : IGameLoopFactory
{
    private readonly ILogger<GameLoopFactory> logger;
    private readonly ILoggerFactory loggerFactory;
    private readonly IGameLoopPhaseRegistry phaseRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameLoopFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory instance.</param>
    /// <param name="phaseRegistry">The phase registry for discovering phases.</param>
    public GameLoopFactory(ILogger<GameLoopFactory> logger, ILoggerFactory loggerFactory, IGameLoopPhaseRegistry phaseRegistry)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        this.phaseRegistry = phaseRegistry ?? throw new ArgumentNullException(nameof(phaseRegistry));
    }

    /// <inheritdoc />
    public IGameLoop CreateGameLoop()
    {
        this.logger.LogDebug("Creating game loop with auto-discovered phases...");

        List<IGameLoopPhase> phases = this.phaseRegistry.GetPhasesByPriority().ToList();

        if (phases.Count == 0)
        {
            this.logger.LogWarning("No game loop phases discovered. Creating empty game loop.");
        }
        else
        {
            this.logger.LogInformation("Creating game loop with {PhaseCount} phases", phases.Count);

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                foreach (IGameLoopPhase? phase in phases)
                {
                    this.logger.LogDebug("  - {PhaseName} (Priority: {Priority})", phase.Name, phase.Priority);
                }
            }
        }

        return new DefaultGameLoop(
            this.loggerFactory.CreateLogger<DefaultGameLoop>(),
            phases);
    }

    /// <inheritdoc />
    public IGameLoop CreateGameLoop(IEnumerable<IGameLoopPhase> additionalPhases)
    {
        this.logger.LogDebug("Creating game loop with auto-discovered and additional phases...");

        List<IGameLoopPhase> discoveredPhases = this.phaseRegistry.GetPhasesByPriority().ToList();
        List<IGameLoopPhase> allPhases = discoveredPhases.Concat(additionalPhases ?? Enumerable.Empty<IGameLoopPhase>())
                                       .OrderByDescending(p => p.Priority)
                                       .ToList();

        this.logger.LogInformation("Creating game loop with {PhaseCount} total phases ({Discovered} discovered, {Additional} additional)",
            allPhases.Count, discoveredPhases.Count, additionalPhases?.Count() ?? 0);

        return new DefaultGameLoop(
            this.loggerFactory.CreateLogger<DefaultGameLoop>(),
            allPhases);
    }

    /// <inheritdoc />
    public IGameLoop CreateGameLoop(params IGameLoopPhase[] additionalPhases) => this.CreateGameLoop((IEnumerable<IGameLoopPhase>)additionalPhases);
}