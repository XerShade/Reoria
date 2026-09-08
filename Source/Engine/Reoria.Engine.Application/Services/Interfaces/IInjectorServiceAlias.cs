namespace Reoria.Engine.Application.Services.Interfaces;

/// <summary>
/// Backwards compatibility alias for the renamed IPhaseService.
/// This interface maintains the old IInjectorService name while delegating to IPhaseService.
/// </summary>
[Obsolete("Use IPhaseService instead. This interface is for backwards compatibility only.")]
public interface IInjectorService : IPhaseService
{
    // All functionality is inherited from IPhaseService
}
