using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Core.Injectors;

/// <summary>
/// Legacy interface - use IGameRender instead.
/// </summary>
[Obsolete("Use IGameRender from Reoria.Engine.Application.Phases instead.")]
public interface IDrawingInjector : IGameRender
{
    // All functionality inherited from IGameRender
}

/// <summary>
/// Legacy interface - use IGamePreRender instead.
/// </summary>
[Obsolete("Use IGamePreRender from Reoria.Engine.Application.Phases instead.")]
public interface IPreDrawingInjector : IGamePreRender
{
    // All functionality inherited from IGamePreRender
}

/// <summary>
/// Legacy interface - use IGamePostRender instead.
/// </summary>
[Obsolete("Use IGamePostRender from Reoria.Engine.Application.Phases instead.")]
public interface IPostDrawingInjector : IGamePostRender
{
    // All functionality inherited from IGamePostRender
}