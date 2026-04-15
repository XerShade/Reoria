using LiteNetLib.Utils;

namespace Reoria.Engine.Network.Packets.Interfaces;

/// <summary>
/// Defines the contract for outgoing network packets that can be sent to remote peers.
/// Implementations of this interface handle the serialization of packet data for transmission.
/// </summary>
public interface IOutgoingPacket
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
    /// Composes the packet data into the specified NetDataWriter for transmission.
    /// This method is called when creating a packet to be sent to remote peers.
    /// </summary>
    /// <param name="writer">The data writer to serialize the packet payload into.</param>
    /// <param name="payload">The data payload to include in the packet.</param>
    /// <returns>The NetDataWriter containing the serialized packet data.</returns>
    NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload);
}