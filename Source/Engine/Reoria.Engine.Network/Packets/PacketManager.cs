using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;
using System.Net;

namespace Reoria.Engine.Network.Packets;

/// <summary>
/// Default implementation of IPacketManager that handles network packet routing, composition, and processing.
/// Provides centralized management for incoming and outgoing packets using registered packet handlers.
/// </summary>
/// <param name="logger">The logger instance for packet-related logging.</param>
/// <param name="incomingPackets">Collection of registered incoming packet handlers.</param>
/// <param name="outgoingPackets">Collection of registered outgoing packet composers.</param>
public class PacketManager(ILogger<IPacketManager> logger, IEnumerable<IIncomingPacket> incomingPackets, IEnumerable<IOutgoingPacket> outgoingPackets) : IPacketManager
{
    /// <summary>
    /// Gets the logger instance used for packet-related logging operations.
    /// </summary>
    protected virtual ILogger<IPacketManager> Logger { get; init; } = logger;

    /// <summary>
    /// Gets the collection of registered incoming packet handlers.
    /// Used for routing incoming packets to their appropriate handlers.
    /// </summary>
    protected virtual List<IIncomingPacket> IncomingPackets { get; init; } = [.. incomingPackets];

    /// <summary>
    /// Gets the collection of registered outgoing packet composers.
    /// Used for serializing outgoing packets for transmission.
    /// </summary>
    protected virtual List<IOutgoingPacket> OutgoingPackets { get; init; } = [.. outgoingPackets];

    /// <summary>
    /// Handles an incoming packet from a connected network peer.
    /// Routes the packet to the appropriate handler based on the packet key.
    /// </summary>
    /// <param name="peer">The network peer that sent the packet.</param>
    /// <param name="reader">The data reader containing the packet payload.</param>
    /// <param name="channel">The delivery channel the packet was received on.</param>
    /// <param name="deliveryMethod">The delivery method used for this packet.</param>
    public virtual void HandleIncomingPacket(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        // Read packet metadata first to identify the packet type.
        string packetKey = reader.GetString();

        // Attempt to find the appropriate packet handler for this packet type.
        IIncomingPacket? incomingPacket = this.IncomingPackets.FirstOrDefault(x => x.PacketKey == packetKey);

        // Process the packet with the found handler, or silently ignore if no handler exists.
        incomingPacket?.ReadPacket(peer, reader);
    }

    /// <summary>
    /// Handles an unconnected packet received from a remote endpoint.
    /// Currently logs a warning as unconnected packets are not supported in this implementation.
    /// </summary>
    /// <param name="remoteEndPoint">The remote endpoint that sent the packet.</param>
    /// <param name="reader">The packet reader containing the packet data.</param>
    /// <param name="messageType">The type of unconnected message received.</param>
    public void HandleIncomingPacket(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        => this.Logger.LogWarning("Received an unconnected packet of size {Size} from {Address} with type {Type}, this is not supported.",
            reader.RawDataSize, remoteEndPoint.ToString(), messageType.ToString());

    /// <summary>
    /// Composes an outgoing packet with the specified key and payload data.
    /// Handles packet serialization and metadata injection for network transmission.
    /// </summary>
    /// <param name="packetKey">The unique key identifying the packet type to compose.</param>
    /// <param name="payload">The data payload to include in the packet.</param>
    /// <returns>A NetDataWriter containing the serialized packet data ready for transmission.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no packet composer is found for the specified key.</exception>
    public virtual NetDataWriter ComposeOutgoingPacket(string packetKey, params object[] payload)
    {
        // Create a new NetDataWriter for the packet data.
        NetDataWriter writer = new();

        // Write packet metadata (the key) first for proper routing on the receiving end.
        writer.Put(packetKey);

        // Find the appropriate packet composer for this packet type.
        IOutgoingPacket? packet = this.OutgoingPackets.FirstOrDefault(x => x.PacketKey == packetKey);

        // Compose the packet using the found composer, or throw an exception if no composer exists.
        return packet != null
            ? packet.ComposePacket(writer, payload)
            : throw new InvalidOperationException("Unable to compose packet with key: " + packetKey);
    }
}