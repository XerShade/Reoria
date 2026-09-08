using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application.Phases;

/// <summary>
/// Base interface for all phase participants.
/// This provides a common contract for all phases (bootstrap, application, game loop).
/// </summary>
public interface IPhaseParticipant
{
    /// <summary>
    /// Gets the name of this phase participant.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a description of what this phase participant does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the dependencies required by this phase participant.
    /// Only other phase participants of the same or earlier phase can be dependencies.
    /// </summary>
    Type[] Dependencies { get; }

    /// <summary>
    /// Gets the platform(s) this phase participant runs on.
    /// </summary>
    Platform Platform { get; }
}
