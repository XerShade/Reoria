using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Legacy interface - use IPhaseParticipant instead.
/// </summary>
/// <remarks>
/// All phase participant interfaces (bootstrap, application, and game loop) inherit from IPhaseParticipant.
/// This provides a consistent way to identify, describe, and manage dependencies for all phase participants.
/// </remarks>
[Obsolete("Use IPhaseParticipant from Reoria.Engine.Application.Phases instead.")]
public interface IInjector : IPhaseParticipant
{
    // All functionality inherited from IPhaseParticipant
}