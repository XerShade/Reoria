using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Core.Injectors;

/// <summary>
/// Legacy interface - use IGameInput instead.
/// </summary>
[Obsolete("Use IGameInput from Reoria.Engine.Application.Phases instead.")]
public interface IInputInjector : IGameInput
{
    // All functionality inherited from IGameInput
}