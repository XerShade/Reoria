using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application.Phases;

/// <summary>
/// Base interface for all application phase participants.
/// Application phase runs after DI container is built - full DI support via constructor injection.
/// </summary>
public interface IApplicationPhase : IPhaseParticipant
{
    // All properties inherited from IPhaseParticipant
}

/// <summary>
/// Application phase for handling application startup.
/// Runs after DI container is built - full DI support via constructor injection.
/// </summary>
public interface IApplicationStart : IApplicationPhase
{
    /// <summary>
    /// Called when the application starts.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    void OnApplicationStart();
}

/// <summary>
/// Application phase for handling application shutdown.
/// Runs after DI container is built - full DI support via constructor injection.
/// </summary>
public interface IApplicationStop : IApplicationPhase
{
    /// <summary>
    /// Called when the application stops.
    /// Full DI support - constructor dependencies are resolved from the container.
    /// </summary>
    void OnApplicationStop();
}
