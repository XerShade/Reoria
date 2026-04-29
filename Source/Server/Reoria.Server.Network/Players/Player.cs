using System.Security.Claims;

namespace Reoria.Server.Network.Players;

/// <summary>
/// Represents a player in the server authoritative multiplayer system.
/// Contains player identity, permissions, and session information.
/// </summary>
public class Player
{
    /// <summary>
    /// Gets the unique identifier for this player.
    /// </summary>
    public Guid PlayerId { get; init; }

    /// <summary>
    /// Gets the username of the player.
    /// </summary>
    public string Username { get; init; }

    /// <summary>
    /// Gets the roles assigned to this player.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; }

    /// <summary>
    /// Gets the permissions granted to this player.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; init; }

    /// <summary>
    /// Gets the session associated with this player.
    /// </summary>
    public Sessions.Session Session { get; init; }

    /// <summary>
    /// Gets the claims-based identity for this player.
    /// </summary>
    public ClaimsPrincipal Identity { get; init; }

    /// <summary>
    /// Gets the timestamp when this player was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp of the last activity for this player.
    /// </summary>
    public DateTime LastActivityAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Player class.
    /// </summary>
    /// <param name="playerId">The unique identifier for the player.</param>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles assigned to the player.</param>
    /// <param name="permissions">The permissions granted to the player.</param>
    /// <param name="session">The session associated with the player.</param>
    public Player(
        Guid playerId,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Sessions.Session session)
    {
        this.PlayerId = playerId;
        this.Username = username ?? throw new ArgumentNullException(nameof(username));
        this.Roles = roles?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(roles));
        this.Permissions = permissions?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(permissions));
        this.Session = session;
        this.CreatedAt = DateTime.UtcNow;
        this.LastActivityAt = DateTime.UtcNow;

        // Create claims-based identity
        List<Claim> claims = new()
        {
            new(ClaimTypes.NameIdentifier, this.PlayerId.ToString()),
            new(ClaimTypes.Name, this.Username),
            new(ClaimTypes.Authentication, DateTime.UtcNow.ToString())
        };

        foreach (string role in this.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (string permission in this.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        this.Identity = new ClaimsPrincipal(new ClaimsIdentity(claims, "ReoriaAuth"));
    }

    /// <summary>
    /// Updates the last activity timestamp to the current UTC time.
    /// </summary>
    public void UpdateLastActivity() => this.LastActivityAt = DateTime.UtcNow;

    /// <summary>
    /// Checks if the player has the specified role.
    /// </summary>
    /// <param name="role">The role to check for.</param>
    /// <returns>True if the player has the role; otherwise, false.</returns>
    public bool HasRole(string role) => this.Roles.Contains(role);

    /// <summary>
    /// Checks if the player has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check for.</param>
    /// <returns>True if the player has the permission; otherwise, false.</returns>
    public bool HasPermission(string permission) => this.Permissions.Contains(permission);

    /// <summary>
    /// Checks if the player is in any of the specified roles.
    /// </summary>
    /// <param name="roles">The roles to check for.</param>
    /// <returns>True if the player is in any of the roles; otherwise, false.</returns>
    public bool IsInAnyRole(params string[] roles) => this.Roles.Any(role => roles.Contains(role));

    /// <summary>
    /// Checks if the player has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check for.</param>
    /// <returns>True if the player has any of the permissions; otherwise, false.</returns>
    public bool HasAnyPermission(params string[] permissions) => this.Permissions.Any(permission => permissions.Contains(permission));
}