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
    /// Gets the dictionary of registered incoming packet handlers keyed by packet key.
    /// Used for O(1) routing of incoming packets to their appropriate handlers.
    /// </summary>
    protected virtual Dictionary<string, IIncomingPacket> IncomingPackets { get; init; } = BuildPacketDictionary(incomingPackets);

    /// <summary>
    /// Gets the dictionary of registered outgoing packet composers keyed by packet key.
    /// Used for O(1) serialization of outgoing packets for transmission.
    /// </summary>
    protected virtual Dictionary<string, IOutgoingPacket> OutgoingPackets { get; init; } = BuildPacketDictionary(outgoingPackets);

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

        // Attempt to find the appropriate packet handler for this packet type using O(1) dictionary lookup.
        if (this.IncomingPackets.TryGetValue(packetKey, out IIncomingPacket? incomingPacket))
        {
            incomingPacket.ReadPacket(peer, reader);
        }
        else
        {
            this.Logger.LogWarning("Received packet with unknown key '{PacketKey}' from {Address}", packetKey, peer.Address.ToString());
        }
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

        // Find the appropriate packet composer for this packet type using O(1) dictionary lookup.
        return this.OutgoingPackets.TryGetValue(packetKey, out IOutgoingPacket? packet)
            ? packet.ComposePacket(writer, payload)
            : throw new InvalidOperationException($"Unable to compose packet with key: {packetKey}");
    }

    /// <summary>
    /// Builds a dictionary mapping packet keys to incoming packet instances for O(1) lookup performance.
    /// Also validates for duplicate packet keys and throws if found.
    /// </summary>
    /// <param name="packets">The enumerable of incoming packet instances to index.</param>
    /// <returns>A dictionary mapping packet keys to incoming packet instances.</returns>
    private static Dictionary<string, IIncomingPacket> BuildPacketDictionary(IEnumerable<IIncomingPacket> packets)
    {
        Dictionary<string, IIncomingPacket> dictionary = [];
        HashSet<string> seenKeys = [];

        foreach (IIncomingPacket packet in packets)
        {
            string key = packet.PacketKey;

            if (seenKeys.Contains(key))
            {
                throw new InvalidOperationException($"Duplicate incoming packet key detected: '{key}'. Multiple packets cannot share the same key.");
            }

            _ = seenKeys.Add(key);
            dictionary[key] = packet;
        }

        return dictionary;
    }

    /// <summary>
    /// Builds a dictionary mapping packet keys to outgoing packet instances for O(1) lookup performance.
    /// Also validates for duplicate packet keys and throws if found.
    /// </summary>
    /// <param name="packets">The enumerable of outgoing packet instances to index.</param>
    /// <returns>A dictionary mapping packet keys to outgoing packet instances.</returns>
    private static Dictionary<string, IOutgoingPacket> BuildPacketDictionary(IEnumerable<IOutgoingPacket> packets)
    {
        Dictionary<string, IOutgoingPacket> dictionary = [];
        HashSet<string> seenKeys = [];

        foreach (IOutgoingPacket packet in packets)
        {
            string key = packet.PacketKey;

            if (seenKeys.Contains(key))
            {
                throw new InvalidOperationException($"Duplicate outgoing packet key detected: '{key}'. Multiple packets cannot share the same key.");
            }

            _ = seenKeys.Add(key);
            dictionary[key] = packet;
        }

        return dictionary;
    }
}