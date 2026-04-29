namespace Reoria.Server.Network.Security.Interfaces;

/// <summary>
/// Defines the contract for managing entity ownership in the server authoritative multiplayer system.
/// </summary>
public interface IEntityOwnershipService
{
    /// <summary>
    /// Sets the owner of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="playerId">The unique identifier of the player who owns the entity.</param>
    /// <returns>True if the ownership was set successfully; otherwise, false.</returns>
    bool SetEntityOwner(Guid entityId, Guid playerId);

    /// <summary>
    /// Gets the owner of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>The player ID of the entity owner, or null if the entity is not owned.</returns>
    Guid? GetEntityOwner(Guid entityId);

    /// <summary>
    /// Checks if an entity is owned by a specific player.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="playerId">The unique identifier of the player to check.</param>
    /// <returns>True if the entity is owned by the player; otherwise, false.</returns>
    bool IsEntityOwnedBy(Guid entityId, Guid playerId);

    /// <summary>
    /// Gets all entities owned by a specific player.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>A collection of entity IDs owned by the player.</returns>
    IReadOnlyCollection<Guid> GetEntitiesOwnedBy(Guid playerId);

    /// <summary>
    /// Removes ownership of an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>True if the ownership was removed successfully; otherwise, false.</returns>
    bool RemoveEntityOwnership(Guid entityId);

    /// <summary>
    /// Transfers ownership of an entity to another player.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="fromPlayerId">The current owner's player ID.</param>
    /// <param name="toPlayerId">The new owner's player ID.</param>
    /// <returns>True if the ownership was transferred successfully; otherwise, false.</returns>
    bool TransferEntityOwnership(Guid entityId, Guid fromPlayerId, Guid toPlayerId);

    /// <summary>
    /// Sets an entity as public (accessible by all players with appropriate permissions).
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="isPublic">Whether the entity should be public.</param>
    /// <returns>True if the public status was set successfully; otherwise, false.</returns>
    bool SetEntityPublic(Guid entityId, bool isPublic);

    /// <summary>
    /// Checks if an entity is public.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>True if the entity is public; otherwise, false.</returns>
    bool IsEntityPublic(Guid entityId);

    /// <summary>
    /// Sets the type information for an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="metadata">Additional metadata about the entity.</param>
    /// <returns>True if the type information was set successfully; otherwise, false.</returns>
    bool SetEntityTypeInfo(Guid entityId, string entityType, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Gets the type information for an entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>The entity type information, or null if not found.</returns>
    EntityTypeInfo? GetEntityTypeInfo(Guid entityId);

    /// <summary>
    /// Removes all entities owned by a specific player (typically called when a player disconnects).
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>The number of entities that were removed.</returns>
    int RemoveEntitiesOwnedBy(Guid playerId);
}

/// <summary>
/// Represents type information for an entity.
/// </summary>
public class EntityTypeInfo
{
    /// <summary>
    /// Gets the type of the entity.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets additional metadata about the entity.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the timestamp when this type information was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Initializes a new instance of the EntityTypeInfo class.
    /// </summary>
    /// <param name="type">The type of the entity.</param>
    /// <param name="metadata">Additional metadata about the entity.</param>
    public EntityTypeInfo(string type, Dictionary<string, object>? metadata = null)
    {
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.Metadata = metadata ?? new Dictionary<string, object>();
        this.CreatedAt = DateTime.UtcNow;
    }
}