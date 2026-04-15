namespace Reoria.Client.Network.Sessions.Interfaces;

/// <summary>
/// Manages client-side session data for server authoritative multiplayer.
/// </summary>
public interface IClientSessionManager
{
    /// <summary>
    /// Gets the current session data.
    /// </summary>
    ClientSessionData? CurrentSession { get; }

    /// <summary>
    /// Creates a new session with the specified session ID.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>The created session data.</returns>
    ClientSessionData CreateSession(Guid sessionId);

    /// <summary>
    /// Initializes the current session with player authentication data.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles assigned to the player.</param>
    /// <param name="permissions">The permissions granted to the player.</param>
    void InitializeSession(Guid playerId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);

    /// <summary>
    /// Gets the current session data.
    /// </summary>
    /// <returns>The current session data, or null if no session exists.</returns>
    ClientSessionData? GetCurrentSession();

    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    /// <returns>True if a session is active; otherwise, false.</returns>
    bool HasActiveSession();

    /// <summary>
    /// Updates the last activity timestamp for the current session.
    /// </summary>
    void UpdateSessionActivity();

    /// <summary>
    /// Adds an entity to the collection of owned entities in the current session.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to add.</param>
    /// <returns>True if the entity was added; false if it was already owned or no session exists.</returns>
    bool AddOwnedEntity(Guid entityId);

    /// <summary>
    /// Removes an entity from the collection of owned entities in the current session.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to remove.</param>
    /// <returns>True if the entity was removed; false if it was not owned or no session exists.</returns>
    bool RemoveOwnedEntity(Guid entityId);

    /// <summary>
    /// Checks if the local player owns the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to check.</param>
    /// <returns>True if the entity is owned by the local player; otherwise, false.</returns>
    bool OwnsEntity(Guid entityId);

    /// <summary>
    /// Gets all entities owned by the local player.
    /// </summary>
    /// <returns>A read-only collection of owned entity IDs.</returns>
    IReadOnlyCollection<Guid> GetOwnedEntities();

    /// <summary>
    /// Clears all owned entities in the current session.
    /// </summary>
    void ClearOwnedEntities();

    /// <summary>
    /// Logs out the current session, clearing authentication and owned entities.
    /// </summary>
    void Logout();

    /// <summary>
    /// Destroys the current session.
    /// </summary>
    void DestroySession();
}
