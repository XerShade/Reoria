using Microsoft.Xna.Framework;
using Reoria.Engine.Core.Components.Interfaces;

namespace Reoria.Engine.Core.Components;

public class TransformComponent : IComponent
{
    public virtual Vector2 Position { get; set; } = Vector2.Zero;
    public virtual Vector2 Scale { get; set; } = Vector2.One;
    public virtual Quaternion Rotation { get; set; } = Quaternion.Identity;
}