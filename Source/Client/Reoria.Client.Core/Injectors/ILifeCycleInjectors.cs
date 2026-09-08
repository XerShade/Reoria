using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Core.Injectors;

// These interfaces are now replaced by the new game loop phase interfaces
// Use IGameInitializeGraphics, IGameLoadContent, IGameInitialize instead

/// <summary>
/// Legacy interface - use IGameInitializeGraphics instead.
/// </summary>
[Obsolete("Use IGameInitializeGraphics from Reoria.Engine.Application.Phases instead.")]
public interface ILifeCycleInitializeGraphicsInjector : IGameInitializeGraphics
{
    // All functionality inherited from IGameInitializeGraphics
}

/// <summary>
/// Legacy interface - use IGameInitialize instead.
/// </summary>
[Obsolete("Use IGameInitialize from Reoria.Engine.Application.Phases instead.")]
public interface ILifeCycleInitializeGameInjector : IGameInitialize
{
    // All functionality inherited from IGameInitialize
}

/// <summary>
/// Legacy interface - use IGameLoadContent instead.
/// </summary>
[Obsolete("Use IGameLoadContent from Reoria.Engine.Application.Phases instead.")]
public interface ILifeCycleLoadContentInjector : IGameLoadContent
{
    // All functionality inherited from IGameLoadContent
}

/// <summary>
/// Legacy interface - no longer needed, functionality moved to IGameInitialize.
/// </summary>
[Obsolete("No longer needed. Use IGameInitialize from Reoria.Engine.Application.Phases instead.")]
public interface ILifeCycleFinalizeInjector : IGameInitialize
{
    // All functionality inherited from IGameInitialize
}