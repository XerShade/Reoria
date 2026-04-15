using LiteNetLib;

namespace Reoria.Server.Network.Sessions.Interfaces;

/// <summary>
/// Manages network sessions for connected clients, providing lifecycle operations for session creation, retrieval, and cleanup.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Closes and removes the session associated with the specified network peer.
    /// </summary>
    /// <param name="peer">The network peer whose session should be closed.</param>
    void Close(NetPeer peer);

    /// <summary>
    /// Determines whether a session exists for the specified network peer.
    /// </summary>
    /// <param name="peer">The network peer to check for an existing session.</param>
    /// <returns>True if a session exists for the peer; otherwise, false.</returns>
    bool HasSession(NetPeer peer);

    /// <summary>
    /// Opens or retrieves a session for the specified network peer.
    /// If a session already exists for the peer, returns the existing session.
    /// Otherwise, creates a new session and returns it.
    /// </summary>
    /// <param name="peer">The network peer to create or retrieve a session for.</param>
    /// <returns>The session associated with the network peer.</returns>
    Session Open(NetPeer peer);
}