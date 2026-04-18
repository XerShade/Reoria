using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using Reoria.Engine.Core.Components;
using Reoria.Game.Entities.Factories.Interfaces;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Engine.Core.Factories;

/// <summary>
/// Factory for creating player entities with default components.
/// </summary>
/// <remarks>
/// This factory implements the IEntityFactory interface to provide standardized
/// player entity creation. It automatically attaches essential components
/// like TransformComponent and NetworkInformationComponent to newly created entities.
/// The factory uses random positioning to spawn players at different locations
/// to prevent overlap and provide variety in spawn locations.
/// </remarks>
public class PlayerEntityFactory(IEntityManager manager) : IEntityFactory
{
    /// <summary>
    /// Gets the entity manager used by this factory.
    /// </summary>
    /// <remarks>
    /// The manager is used to create new entities and attach components.
    /// This property is protected to allow derived factories to customize
    /// entity creation behavior while maintaining access to the manager.
    /// </remarks>
    protected virtual IEntityManager Manager { get; set; } = manager;
    
    /// <summary>
    /// Gets the random number generator used by this factory.
    /// </summary>
    /// <remarks>
    /// Random instance is used for generating spawn positions and other
    /// randomized entity properties. Using a shared Random instance ensures
    /// consistent randomness across factory calls. This property is protected
    /// to allow derived factories to use different randomization strategies.
    /// </remarks>
    protected virtual Random Random { get; set; } = new Random();

    /// <summary>
    /// Creates a new player entity with default components.
    /// </summary>
    /// <returns>A new entity with TransformComponent and NetworkInformationComponent attached.</returns>
    /// <remarks>
    /// This method creates a player entity and automatically attaches the essential
    /// components needed for multiplayer functionality. The entity is positioned
    /// randomly within a predefined spawn area to prevent clustering.
    /// The NetworkInformationComponent is initialized with OwnerId = -1,
    /// indicating it needs to be assigned by the server when the player
    /// authenticates and connects.
    /// </remarks>
    public Entity Create()
    {
        Entity entity = this.Manager.CreateEntity();

        // Attach transform component with random spawn position
        entity.Attach(new TransformComponent()
        {
            Position = new Vector2(this.Random.Next(100, 1080 - 100), this.Random.Next(100, 1920 - 100))
        });

        // Attach network information component with unassigned owner
        entity.Attach(new NetworkInformationComponent()
        {
            // OwnerId will be set by server when player authenticates
            // -1 indicates unassigned/awaiting ownership
        });

        return entity;
    }
}