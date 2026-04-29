using LiteNetLib;
using Reoria.Server.Network.Players.Interfaces;
using Reoria.Server.Network.Sessions;

namespace Reoria.Server.Network.Players;

/// <summary>
/// Default implementation of IPlayerManager that manages player instances using thread-safe collections.
/// Provides comprehensive player lifecycle management for server authoritative multiplayer.
/// </summary>
public class PlayerManager : IPlayerManager
{
    private readonly object lockObject = new();
    private readonly Dictionary<Guid, Player> playersById = new();
    private readonly Dictionary<NetPeer, Player> playersByPeer = new();
    private readonly Dictionary<Session, Player> playersBySession = new();

    /// <summary>
    /// Gets a player by their unique identifier.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    public Player? GetPlayer(Guid playerId)
    {
        lock (this.lockObject)
        {
            return this.playersById.TryGetValue(playerId, out Player? player) ? player : null;
        }
    }

    /// <summary>
    /// Gets a player by their session.
    /// </summary>
    /// <param name="session">The session associated with the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    public Player? GetPlayerBySession(Session session)
    {
        lock (this.lockObject)
        {
            return this.playersBySession.TryGetValue(session, out Player? player) ? player : null;
        }
    }

    /// <summary>
    /// Gets a player by their network peer.
    /// </summary>
    /// <param name="peer">The network peer associated with the player.</param>
    /// <returns>The player if found; otherwise, null.</returns>
    public Player? GetPlayerByPeer(NetPeer peer)
    {
        lock (this.lockObject)
        {
            return this.playersByPeer.TryGetValue(peer, out Player? player) ? player : null;
        }
    }

    /// <summary>
    /// Creates a new player with the specified information.
    /// </summary>
    /// <param name="username">The username of the player.</param>
    /// <param name="roles">The roles to assign to the player.</param>
    /// <param name="permissions">The permissions to grant to the player.</param>
    /// <param name="session">The session associated with the player.</param>
    /// <returns>The created player.</returns>
    /// <exception cref="ArgumentException">Thrown when a player already exists for the given session or peer.</exception>
    public Player CreatePlayer(string username, IEnumerable<string> roles, IEnumerable<string> permissions, Session session)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
        }

        if (roles == null)
        {
            throw new ArgumentNullException(nameof(roles));
        }

        if (permissions == null)
        {
            throw new ArgumentNullException(nameof(permissions));
        }

        lock (this.lockObject)
        {
            // Check if player already exists for this session or peer
            if (this.playersBySession.ContainsKey(session))
            {
                throw new ArgumentException("A player already exists for the specified session.", nameof(session));
            }

            if (this.playersByPeer.ContainsKey(session.Peer))
            {
                throw new ArgumentException("A player already exists for the specified peer.", nameof(session));
            }

            // Create new player
            Player player = new(
                Guid.NewGuid(),
                username,
                roles,
                permissions,
                session);

            // Add to all lookup dictionaries
            this.playersById[player.PlayerId] = player;
            this.playersByPeer[session.Peer] = player;
            this.playersBySession[session] = player;

            return player;
        }
    }

    /// <summary>
    /// Removes a player from the manager.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise, false.</returns>
    public bool RemovePlayer(Guid playerId)
    {
        lock (this.lockObject)
        {
            if (!this.playersById.TryGetValue(playerId, out Player? player))
            {
                return false;
            }

            // Remove from all lookup dictionaries
            _ = this.playersById.Remove(playerId);
            _ = this.playersByPeer.Remove(player.Session.Peer);
            _ = this.playersBySession.Remove(player.Session);

            return true;
        }
    }

    /// <summary>
    /// Removes a player by their network peer.
    /// </summary>
    /// <param name="peer">The network peer of the player to remove.</param>
    /// <returns>True if the player was removed; otherwise, false.</returns>
    public bool RemovePlayerByPeer(NetPeer peer)
    {
        lock (this.lockObject)
        {
            if (!this.playersByPeer.TryGetValue(peer, out Player? player))
            {
                return false;
            }

            // Remove from all lookup dictionaries
            _ = this.playersById.Remove(player.PlayerId);
            _ = this.playersByPeer.Remove(peer);
            _ = this.playersBySession.Remove(player.Session);

            return true;
        }
    }

    /// <summary>
    /// Gets all currently connected players.
    /// </summary>
    /// <returns>A read-only collection of all connected players.</returns>
    public IReadOnlyCollection<Player> GetAllPlayers()
    {
        lock (this.lockObject)
        {
            return this.playersById.Values.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Checks if a player with the specified identifier exists.
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    /// <returns>True if the player exists; otherwise, false.</returns>
    public bool HasPlayer(Guid playerId)
    {
        lock (this.lockObject)
        {
            return this.playersById.ContainsKey(playerId);
        }
    }

    /// <summary>
    /// Checks if a player exists for the specified network peer.
    /// </summary>
    /// <param name="peer">The network peer to check.</param>
    /// <returns>True if a player exists for the peer; otherwise, false.</returns>
    public bool HasPlayerForPeer(NetPeer peer)
    {
        lock (this.lockObject)
        {
            return this.playersByPeer.ContainsKey(peer);
        }
    }
}