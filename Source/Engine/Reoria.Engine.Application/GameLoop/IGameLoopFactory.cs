namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// Defines the contract for a game loop factory service.
/// </summary>
public interface IGameLoopFactory
{
    /// <summary>
    /// Creates a game loop with auto-discovered phases from the DI container.
    /// </summary>
    /// <returns>A configured game loop instance.</returns>
    IGameLoop CreateGameLoop();

    /// <summary>
    /// Creates a game loop with auto-discovered phases plus additional phases.
    /// </summary>
    /// <param name="additionalPhases">Additional phases to include beyond auto-discovered ones.</param>
    /// <returns>A configured game loop instance.</returns>
    IGameLoop CreateGameLoop(IEnumerable<IGameLoopPhase> additionalPhases);

    /// <summary>
    /// Creates a game loop with auto-discovered phases plus additional phases.
    /// </summary>
    /// <param name="additionalPhases">Additional phases to include beyond auto-discovered ones.</param>
    /// <returns>A configured game loop instance.</returns>
    IGameLoop CreateGameLoop(params IGameLoopPhase[] additionalPhases);
}
