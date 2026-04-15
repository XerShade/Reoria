using Reoria.Server.Network.Security.Interfaces;

namespace Reoria.Server.Network.Security;

/// <summary>
/// Default implementation of IEntityOwnershipService that manages entity ownership
/// using thread-safe collections for server authoritative multiplayer.
/// </summary>
public class EntityOwnershipService : IEntityOwnershipService
{
    private readonly object lockObject = new();
    private readonly Dictionary<Guid, Guid> entityOwners = new();
    private readonly Dictionary<Guid, HashSet<Guid>> playerEntities = new();
    private readonly Dictionary<Guid, bool> publicEntities = new();
    private readonly Dictionary<Guid, EntityTypeInfo> entityTypes = new();

    /// <summary>
    /// Sets the owner of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="playerId">The unique identifier of the player who owns the entity.</param>
    /// <returns>True if the ownership was set successfully; otherwise, false.</returns>
    public bool SetEntityOwner(Guid entityId, Guid playerId)
    {
        if (entityId == Guid.Empty)
            return false;

        if (playerId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            // Remove entity from previous owner if it exists
            if (entityOwners.TryGetValue(entityId, out var previousOwnerId))
            {
                if (playerEntities.TryGetValue(previousOwnerId, out var previousEntities))
                {
                    previousEntities.Remove(entityId);
                    if (previousEntities.Count == 0)
                    {
                        playerEntities.Remove(previousOwnerId);
                    }
                }
            }

            // Set new owner
            entityOwners[entityId] = playerId;

            if (!playerEntities.TryGetValue(playerId, out var currentEntities))
            {
                currentEntities = new HashSet<Guid>();
                playerEntities[playerId] = currentEntities;
            }

            currentEntities.Add(entityId);

            return true;
        }
    }

    /// <summary>
    /// Gets the owner of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>The player ID of the entity owner, or null if the entity is not owned.</returns>
    public Guid? GetEntityOwner(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return null;

        lock (lockObject)
        {
            return entityOwners.TryGetValue(entityId, out var ownerId) ? ownerId : null;
        }
    }

    /// <summary>
    /// Checks if an entity is owned by a specific player.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="playerId">The unique identifier of the player to check.</param>
    /// <returns>True if the entity is owned by the player; otherwise, false.</returns>
    public bool IsEntityOwnedBy(Guid entityId, Guid playerId)
    {
        if (entityId == Guid.Empty || playerId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            return entityOwners.TryGetValue(entityId, out var ownerId) && ownerId == playerId;
        }
    }

    /// <summary>
    /// Gets all entities owned by a specific player.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>A collection of entity IDs owned by the player.</returns>
    public IReadOnlyCollection<Guid> GetEntitiesOwnedBy(Guid playerId)
    {
        if (playerId == Guid.Empty)
            return Array.Empty<Guid>();

        lock (lockObject)
        {
            return playerEntities.TryGetValue(playerId, out var entities) 
                ? entities.ToList().AsReadOnly() 
                : Array.Empty<Guid>();
        }
    }

    /// <summary>
    /// Removes ownership of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>True if the ownership was removed successfully; otherwise, false.</returns>
    public bool RemoveEntityOwnership(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            if (!entityOwners.TryGetValue(entityId, out var ownerId))
                return false;

            // Remove from owner mapping
            entityOwners.Remove(entityId);

            // Remove from player's entity collection
            if (playerEntities.TryGetValue(ownerId, out var entities))
            {
                entities.Remove(entityId);
                if (entities.Count == 0)
                {
                    playerEntities.Remove(ownerId);
                }
            }

            // Remove from public entities
            publicEntities.Remove(entityId);

            // Remove from entity types
            entityTypes.Remove(entityId);

            return true;
        }
    }

    /// <summary>
    /// Transfers ownership of an entity to another player.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="fromPlayerId">The current owner's player ID.</param>
    /// <param name="toPlayerId">The new owner's player ID.</param>
    /// <returns>True if the ownership was transferred successfully; otherwise, false.</returns>
    public bool TransferEntityOwnership(Guid entityId, Guid fromPlayerId, Guid toPlayerId)
    {
        if (entityId == Guid.Empty || fromPlayerId == Guid.Empty || toPlayerId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            // Verify current ownership
            if (!entityOwners.TryGetValue(entityId, out var currentOwnerId) || currentOwnerId != fromPlayerId)
                return false;

            // Remove from current owner
            if (playerEntities.TryGetValue(fromPlayerId, out var fromEntities))
            {
                fromEntities.Remove(entityId);
                if (fromEntities.Count == 0)
                {
                    playerEntities.Remove(fromPlayerId);
                }
            }

            // Add to new owner
            entityOwners[entityId] = toPlayerId;

            if (!playerEntities.TryGetValue(toPlayerId, out var toEntities))
            {
                toEntities = new HashSet<Guid>();
                playerEntities[toPlayerId] = toEntities;
            }

            toEntities.Add(entityId);

            return true;
        }
    }

    /// <summary>
    /// Sets an entity as public (accessible by all players with appropriate permissions).
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="isPublic">Whether the entity should be public.</param>
    /// <returns>True if the public status was set successfully; otherwise, false.</returns>
    public bool SetEntityPublic(Guid entityId, bool isPublic)
    {
        if (entityId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            if (isPublic)
            {
                publicEntities[entityId] = true;
            }
            else
            {
                publicEntities.Remove(entityId);
            }

            return true;
        }
    }

    /// <summary>
    /// Checks if an entity is public.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>True if the entity is public; otherwise, false.</returns>
    public bool IsEntityPublic(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return false;

        lock (lockObject)
        {
            return publicEntities.ContainsKey(entityId);
        }
    }

    /// <summary>
    /// Sets the type information for an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="metadata">Additional metadata about the entity.</param>
    /// <returns>True if the type information was set successfully; otherwise, false.</returns>
    public bool SetEntityTypeInfo(Guid entityId, string entityType, Dictionary<string, object>? metadata = null)
    {
        if (entityId == Guid.Empty || string.IsNullOrWhiteSpace(entityType))
            return false;

        lock (lockObject)
        {
            entityTypes[entityId] = new EntityTypeInfo(entityType, metadata);
            return true;
        }
    }

    /// <summary>
    /// Gets the type information for an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>The entity type information, or null if not found.</returns>
    public EntityTypeInfo? GetEntityTypeInfo(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return null;

        lock (lockObject)
        {
            return entityTypes.TryGetValue(entityId, out var typeInfo) ? typeInfo : null;
        }
    }

    /// <summary>
    /// Removes all entities owned by a specific player (typically called when a player disconnects).
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>The number of entities that were removed.</returns>
    public int RemoveEntitiesOwnedBy(Guid playerId)
    {
        if (playerId == Guid.Empty)
            return 0;

        lock (lockObject)
        {
            if (!playerEntities.TryGetValue(playerId, out var entities))
                return 0;

            var entityIds = entities.ToList();
            var removedCount = 0;

            foreach (var entityId in entityIds)
            {
                if (RemoveEntityOwnership(entityId))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }
    }
}
