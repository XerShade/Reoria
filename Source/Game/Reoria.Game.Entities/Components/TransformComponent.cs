using Microsoft.Xna.Framework;
using Reoria.Engine.Core.Components.Interfaces;

namespace Reoria.Engine.Core.Components;

/// <summary>
/// Represents the transform (position, rotation, scale) of an entity in 2D space.
/// </summary>
/// <remarks>
/// This component is fundamental for all entities that need to be positioned or rendered.
/// It provides the basic spatial properties required for entity placement and rendering.
/// The TransformComponent is typically used by rendering systems to determine where
/// and how to draw entities, and by physics systems for collision detection.
/// All properties are virtual to allow for derived transform components with
/// additional functionality or different coordinate systems.
/// </remarks>
public class TransformComponent : IComponent
{
    /// <summary>
    /// Gets or sets the 2D position of the entity.
    /// </summary>
    /// <remarks>
    /// Position represents the entity's location in world space coordinates.
    /// Vector2.Zero (0, 0) is used as default, typically indicating
    /// the entity spawns at the world origin. Position is updated by movement
    /// systems, animation systems, or when entities are explicitly placed.
    /// </remarks>
    public virtual Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the 2D scale of the entity.
    /// </summary>
    /// <remarks>
    /// Scale determines the size multiplier for the entity's rendering.
    /// Vector2.One (1, 1) is used as default, representing normal scale.
    /// Scale affects how large the entity appears but doesn't change its actual
    /// collision bounds unless physics systems are updated accordingly.
    /// </remarks>
    public virtual Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets the rotation of the entity as a quaternion.
    /// </summary>
    /// <remarks>
    /// Rotation represents the entity's orientation in 3D space, even for 2D games.
    /// Quaternion.Identity (no rotation) is used as default, indicating the entity
    /// faces its default orientation. For 2D games, this typically represents
    /// rotation around the Z-axis. Rotation is used by rendering systems for
    /// proper sprite orientation and by physics systems for angular calculations.
    /// </remarks>
    public virtual Quaternion Rotation { get; set; } = Quaternion.Identity;
}