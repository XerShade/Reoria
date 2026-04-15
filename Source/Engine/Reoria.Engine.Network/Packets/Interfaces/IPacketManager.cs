using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace Reoria.Engine.Network.Packets.Interfaces;

/// <summary>
/// Defines the contract for managing network packet routing, composition, and handling.
/// Provides a centralized interface for processing incoming packets and composing outgoing packets.
/// </summary>
public interface IPacketManager
{
    /// <summary>
    /// Composes an outgoing packet with the specified key and payload data.
    /// This method handles packet serialization and metadata injection.
    /// </summary>
    /// <param name="packetKey">The unique key identifying the packet type to compose.</param>
    /// <param name="payload">The data payload to include in the packet.</param>
    /// <returns>A NetDataWriter containing the serialized packet data ready for transmission.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no packet composer is found for the specified key.</exception>
    NetDataWriter ComposeOutgoingPacket(string packetKey, params object[] payload);

    /// <summary>
    /// Handles an incoming packet from a connected network peer.
    /// Routes the packet to the appropriate handler based on the packet key.
    /// </summary>
    /// <param name="peer">The network peer that sent the packet.</param>
    /// <param name="reader">The data reader containing the packet payload.</param>
    /// <param name="channel">The delivery channel the packet was received on.</param>
    /// <param name="deliveryMethod">The delivery method used for this packet.</param>
    void HandleIncomingPacket(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod);

    /// <summary>
    /// Handles an unconnected packet received from a remote endpoint.
    /// Currently logs a warning as unconnected packets are not supported.
    /// </summary>
    /// <param name="remoteEndPoint">The remote endpoint that sent the packet.</param>
    /// <param name="reader">The packet reader containing the packet data.</param>
    /// <param name="messageType">The type of unconnected message received.</param>
    void HandleIncomingPacket(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
}