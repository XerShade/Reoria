using Reoria.Client.Network.Sessions.Interfaces;

namespace Reoria.Client.Network.Sessions;

/// <summary>
/// Default implementation of IClientSessionManager that manages client-side session data
/// for server authoritative multiplayer.
/// </summary>
public class ClientSessionManager : IClientSessionManager
{
    /// <summary>
    /// Gets the current session data.
    /// </summary>
    public ClientSessionData? CurrentSession { get; private set; }

    /// <summary>
    /// Creates a new session with the specified session ID.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>The created session data.</returns>
    public ClientSessionData CreateSession(Guid sessionId)
    {
        // Destroy any existing session
        this.DestroySession();

        this.CurrentSession = new ClientSessionData(sessionId);
        return this.CurrentSession;
    }

    /// <summary>
    /// Initializes the current session with player authentication data.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles assigned to the player.</param>
    /// <param name="permissions">The permissions granted to the player.</param>
    /// <exception cref="InvalidOperationException">Thrown when no session is active.</exception>
    public void InitializeSession(Guid playerId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        if (this.CurrentSession == null)
        {
            throw new InvalidOperationException("No active session to initialize.");
        }

        this.CurrentSession.InitializePlayer(playerId, username, roles, permissions);
    }

    /// <summary>
    /// Gets the current session data.
    /// </summary>
    /// <returns>The current session data, or null if no session exists.</returns>
    public ClientSessionData? GetCurrentSession() => this.CurrentSession;

    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    /// <returns>True if a session is active; otherwise, false.</returns>
    public bool HasActiveSession() => this.CurrentSession != null;

    /// <summary>
    /// Updates the last activity timestamp for the current session.
    /// </summary>
    public void UpdateSessionActivity() => this.CurrentSession?.UpdateLastActivity();

    /// <summary>
    /// Adds an entity to the collection of owned entities in the current session.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to add.</param>
    /// <returns>True if the entity was added; false if it was already owned or no session exists.</returns>
    public bool AddOwnedEntity(Guid entityId) => this.CurrentSession?.AddOwnedEntity(entityId) ?? false;

    /// <summary>
    /// Removes an entity from the collection of owned entities in the current session.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to remove.</param>
    /// <returns>True if the entity was removed; false if it was not owned or no session exists.</returns>
    public bool RemoveOwnedEntity(Guid entityId) => this.CurrentSession?.RemoveOwnedEntity(entityId) ?? false;

    /// <summary>
    /// Checks if the local player owns the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to check.</param>
    /// <returns>True if the entity is owned by the local player; otherwise, false.</returns>
    public bool OwnsEntity(Guid entityId) => this.CurrentSession?.OwnsEntity(entityId) ?? false;

    /// <summary>
    /// Gets all entities owned by the local player.
    /// </summary>
    /// <returns>A read-only collection of owned entity IDs.</returns>
    public IReadOnlyCollection<Guid> GetOwnedEntities() => this.CurrentSession?.OwnedEntities ?? Array.Empty<Guid>();

    /// <summary>
    /// Clears all owned entities in the current session.
    /// </summary>
    public void ClearOwnedEntities() => this.CurrentSession?.ClearOwnedEntities();

    /// <summary>
    /// Logs out the current session, clearing authentication and owned entities.
    /// </summary>
    public void Logout() => this.CurrentSession?.Logout();

    /// <summary>
    /// Destroys the current session.
    /// </summary>
    public void DestroySession()
        => this.CurrentSession = null;
}