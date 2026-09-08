namespace Reoria.Engine.Application.Services;

/// <summary>
/// Backwards compatibility alias for the renamed PhaseService.
/// This class maintains the old InjectorService name while delegating to PhaseService.
/// </summary>
[Obsolete("Use PhaseService instead. This class is for backwards compatibility only.")]
public partial class InjectorService : PhaseService
{
    // All functionality is inherited from PhaseService
}
