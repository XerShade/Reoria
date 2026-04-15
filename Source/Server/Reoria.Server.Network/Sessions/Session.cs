using LiteNetLib;

namespace Reoria.Server.Network.Sessions;

/// <summary>
/// Represents a network session for a connected client, containing a unique identifier and the associated network peer.
/// This struct is immutable to ensure thread safety and prevent accidental modification of session state.
/// </summary>
/// <remarks>
/// Initializes a new session with the specified GUID and network peer.
/// </remarks>
/// <param name="guid">The unique identifier for this session.</param>
/// <param name="peer">The network peer associated with this session.</param>
/// <exception cref="ArgumentNullException">Thrown when the peer parameter is null.</exception>
public readonly struct Session(Guid guid, NetPeer peer)
{
    /// <summary>
    /// Gets the unique identifier for this session.
    /// Automatically generates a new GUID if not explicitly provided.
    /// </summary>
    public Guid Guid { get; init; } = guid;

    /// <summary>
    /// Gets the network peer associated with this session.
    /// The peer represents the client connection and is used for network communication.
    /// </summary>
    public NetPeer Peer { get; init; } = peer ?? throw new ArgumentNullException(nameof(peer));

    /// <summary>
    /// Initializes a new session with the specified network peer and an automatically generated GUID.
    /// </summary>
    /// <param name="peer">The network peer associated with this session.</param>
    public Session(NetPeer peer)
        : this(Guid.NewGuid(), peer) { }
}