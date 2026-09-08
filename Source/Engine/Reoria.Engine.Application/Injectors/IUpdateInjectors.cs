using Microsoft.Xna.Framework;
using Reoria.Engine.Application.Phases;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Legacy interface - use IGameVariableUpdate instead.
/// </summary>
[Obsolete("Use IGameVariableUpdate from Reoria.Engine.Application.Phases instead.")]
public interface IVariableUpdateInjector : IGameVariableUpdate
{
    // All functionality inherited from IGameVariableUpdate
}

/// <summary>
/// Legacy interface - use IGameFixedUpdate instead.
/// </summary>
[Obsolete("Use IGameFixedUpdate from Reoria.Engine.Application.Phases instead.")]
public interface IFixedUpdateInjector : IGameFixedUpdate
{
    // All functionality inherited from IGameFixedUpdate
}
