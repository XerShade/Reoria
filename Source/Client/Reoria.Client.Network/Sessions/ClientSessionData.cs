namespace Reoria.Client.Network.Sessions;

/// <summary>
/// Represents client-side session data that tracks the local player's state,
/// owned entities, and permissions for server authoritative multiplayer.
/// </summary>
public class ClientSessionData
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the local player.
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Gets the username of the local player.
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Gets the roles assigned to the local player.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; private set; }

    /// <summary>
    /// Gets the permissions granted to the local player.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; private set; }

    /// <summary>
    /// Gets the collection of entity IDs owned by the local player.
    /// This will be used for future ECS integration to track owned entities.
    /// </summary>
    public IReadOnlyCollection<Guid> OwnedEntities => ownedEntities.AsReadOnly();

    private readonly HashSet<Guid> ownedEntities = new();

    /// <summary>
    /// Gets the timestamp when this session was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp of the last activity for this session.
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    /// <summary>
    /// Gets whether the session is currently authenticated.
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ClientSessionData class.
    /// </summary>
    /// <param name="sessionId">The unique identifier for this session.</param>
    public ClientSessionData(Guid sessionId)
    {
        SessionId = sessionId;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        IsAuthenticated = false;
        Username = string.Empty;
        Roles = Array.Empty<string>();
        Permissions = Array.Empty<string>();
    }

    /// <summary>
    /// Initializes the session with player authentication data.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles assigned to the player.</param>
    /// <param name="permissions">The permissions granted to the player.</param>
    public void InitializePlayer(Guid playerId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        PlayerId = playerId;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Roles = roles?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(roles));
        Permissions = permissions?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(permissions));
        IsAuthenticated = true;
        UpdateLastActivity();
    }

    /// <summary>
    /// Updates the last activity timestamp to the current UTC time.
    /// </summary>
    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an entity to the collection of owned entities.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to add.</param>
    /// <returns>True if the entity was added; false if it was already owned.</returns>
    public bool AddOwnedEntity(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return false;

        var added = ownedEntities.Add(entityId);
        if (added)
        {
            UpdateLastActivity();
        }
        return added;
    }

    /// <summary>
    /// Removes an entity from the collection of owned entities.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to remove.</param>
    /// <returns>True if the entity was removed; false if it was not owned.</returns>
    public bool RemoveOwnedEntity(Guid entityId)
    {
        if (entityId == Guid.Empty)
            return false;

        var removed = ownedEntities.Remove(entityId);
        if (removed)
        {
            UpdateLastActivity();
        }
        return removed;
    }

    /// <summary>
    /// Checks if the local player owns the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to check.</param>
    /// <returns>True if the entity is owned by the local player; otherwise, false.</returns>
    public bool OwnsEntity(Guid entityId)
    {
        return entityId != Guid.Empty && ownedEntities.Contains(entityId);
    }

    /// <summary>
    /// Clears all owned entities (typically called when reconnecting or logging out).
    /// </summary>
    public void ClearOwnedEntities()
    {
        ownedEntities.Clear();
        UpdateLastActivity();
    }

    /// <summary>
    /// Checks if the local player has the specified role.
    /// </summary>
    /// <param name="role">The role to check for.</param>
    /// <returns>True if the player has the role; otherwise, false.</returns>
    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }

    /// <summary>
    /// Checks if the local player has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check for.</param>
    /// <returns>True if the player has the permission; otherwise, false.</returns>
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    /// <summary>
    /// Checks if the local player is in any of the specified roles.
    /// </summary>
    /// <param name="roles">The roles to check for.</param>
    /// <returns>True if the player is in any of the roles; otherwise, false.</returns>
    public bool IsInAnyRole(params string[] roles)
    {
        return Roles.Any(role => roles.Contains(role));
    }

    /// <summary>
    /// Checks if the local player has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check for.</param>
    /// <returns>True if the player has any of the permissions; otherwise, false.</returns>
    public bool HasAnyPermission(params string[] permissions)
    {
        return Permissions.Any(permission => permissions.Contains(permission));
    }

    /// <summary>
    /// Logs out the current session, clearing authentication and owned entities.
    /// </summary>
    public void Logout()
    {
        IsAuthenticated = false;
        PlayerId = Guid.Empty;
        Username = string.Empty;
        Roles = Array.Empty<string>();
        Permissions = Array.Empty<string>();
        ClearOwnedEntities();
        UpdateLastActivity();
    }
}
