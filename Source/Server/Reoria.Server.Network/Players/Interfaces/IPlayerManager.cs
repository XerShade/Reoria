using LiteNetLib;

namespace Reoria.Server.Network.Players.Interfaces;

/// <summary>
/// Manages player instances and their lifecycle in the server authoritative multiplayer system.
/// </summary>
public interface IPlayerManager
{
    /// <summary>
    /// Gets a player by their unique identifier.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    Player? GetPlayer(Guid playerId);

    /// <summary>
    /// Gets a player by their session.
    /// </summary>
    /// <param name="session">The session associated with the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    Player? GetPlayerBySession(Sessions.Session session);

    /// <summary>
    /// Gets a player by their network peer.
    /// </summary>
    /// <param name="peer">The network peer associated with the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    Player? GetPlayerByPeer(NetPeer peer);

    /// <summary>
    /// Creates a new player with the specified information.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles to assign to the player.</param>
    /// <param name="permissions">The permissions to grant to the player.</param>
    /// <param name="session">The session associated with the player.</param>
    /// <returns>The created player.</returns>
    Player CreatePlayer(string username, IEnumerable<string> roles, IEnumerable<string> permissions, Sessions.Session session);

    /// <summary>
    /// Removes a player from the manager.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise, false.</returns>
    bool RemovePlayer(Guid playerId);

    /// <summary>
    /// Removes a player by their network peer.
    /// </summary>
    /// <param name="peer">The network peer of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise, false.</returns>
    bool RemovePlayerByPeer(NetPeer peer);

    /// <summary>
    /// Gets all currently connected players.
    /// </summary>
    /// <returns>A read-only collection of all connected players.</returns>
    IReadOnlyCollection<Player> GetAllPlayers();

    /// <summary>
    /// Checks if a player with the specified identifier exists.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>True if the player exists; otherwise, false.</returns>
    bool HasPlayer(Guid playerId);

    /// <summary>
    /// Checks if a player exists for the specified network peer.
    /// </summary>
    /// <param name="peer">The network peer to check.</param>
    /// <returns>True if a player exists for the peer; otherwise, false.</returns>
    bool HasPlayerForPeer(NetPeer peer);
}
