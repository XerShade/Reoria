using LiteNetLib;
using Reoria.Server.Network.Sessions.Interfaces;

namespace Reoria.Server.Network.Sessions;

/// <summary>
/// Default implementation of ISessionManager that manages network sessions using an in-memory collection.
/// Provides thread-safe operations for creating, retrieving, and closing client sessions.
/// </summary>
public class SessionManager : ISessionManager
{
    /// <summary>
    /// The internal collection of active sessions.
    /// Protected to allow derived classes to access the session collection directly.
    /// </summary>
    protected List<Session> Sessions = [];

    /// <summary>
    /// Determines whether a session exists for the specified network peer.
    /// </summary>
    /// <param name="peer">The network peer to check for an existing session.</param>
    /// <returns>True if a session exists for the peer; otherwise, false.</returns>
    public virtual bool HasSession(NetPeer peer) 
        => this.Sessions.Any(s => s.Peer == peer);

    /// <summary>
    /// Opens or retrieves a session for the specified network peer.
    /// If a session already exists for the peer, returns the existing session.
    /// Otherwise, creates a new session with a unique GUID and adds it to the session collection.
    /// </summary>
    /// <param name="peer">The network peer to create or retrieve a session for.</param>
    /// <returns>The session associated with the network peer.</returns>
    public virtual Session Open(NetPeer peer)
    {
        // Check if a session exists for the peer.
        if (this.HasSession(peer))
        {
            // Retrieve the existing session.
            Session existingSession = this.Sessions.FirstOrDefault(s => s.Peer == peer);
            return existingSession;
        }

        // Create a new session and add it to the collection.
        Session session = new(peer);
        this.Sessions.Add(session);

        // Return the newly created session.
        return session;
    }

    /// <summary>
    /// Closes and removes all sessions associated with the specified network peer.
    /// This method removes all matching sessions from the internal collection.
    /// </summary>
    /// <param name="peer">The network peer whose sessions should be closed.</param>
    public virtual void Close(NetPeer peer) 
        => this.Sessions.RemoveAll(s => s.Peer == peer);
}