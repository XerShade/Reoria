namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// Defines the contract for a game loop phase registry service.
/// </summary>
public interface IGameLoopPhaseRegistry
{
    /// <summary>
    /// Gets all registered game loop phases.
    /// </summary>
    /// <returns>A collection of all registered phases.</returns>
    IEnumerable<IGameLoopPhase> GetPhases();

    /// <summary>
    /// Gets all enabled game loop phases.
    /// </summary>
    /// <returns>A collection of enabled phases.</returns>
    IEnumerable<IGameLoopPhase> GetEnabledPhases();

    /// <summary>
    /// Gets all enabled game loop phases sorted by priority (highest first).
    /// </summary>
    /// <returns>A collection of enabled phases ordered by priority.</returns>
    IEnumerable<IGameLoopPhase> GetPhasesByPriority();
}
