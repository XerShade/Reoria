using LiteNetLib;
using LiteNetLib.Utils;

namespace Reoria.Engine.Network.Packets.Interfaces;

/// <summary>
/// Defines the contract for incoming network packets that can be received from remote peers.
/// Implementations of this interface handle the deserialization and processing of packet data.
/// </summary>
public interface IIncomingPacket
{
    /// <summary>
    /// Gets the unique key that identifies this packet type.
    /// This key must match between the sender and receiver for proper packet routing.
    /// </summary>
    string PacketKey { get; }

    /// <summary>
    /// Gets the human-readable name of this packet type.
    /// Used for logging, debugging, and UI display purposes.
    /// </summary>
    string PacketName { get; }

    /// <summary>
    /// Gets a detailed description of what this packet does and when it's used.
    /// Provides context for developers working with the packet system.
    /// </summary>
    string PacketDescription { get; }

    /// <summary>
    /// Reads and processes the incoming packet data from the specified network peer.
    /// This method is called when a packet with the matching PacketKey is received.
    /// </summary>
    /// <param name="sender">The network peer that sent this packet.</param>
    /// <param name="reader">The data reader containing the packet payload.</param>
    void ReadPacket(NetPeer sender, NetDataReader reader);
}