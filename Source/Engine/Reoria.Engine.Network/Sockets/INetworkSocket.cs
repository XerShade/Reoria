using LiteNetLib;
using LiteNetLib.Utils;

namespace Reoria.Engine.Network.Sockets;

/// <summary>
/// Defines the contract for network socket functionality with packet sending capabilities.
/// Provides type-safe packet sending using generic types and flexible delivery methods.
/// </summary>
public interface INetworkSocket : IDisposable
{
    /// <summary>
    /// Gets whether the socket is currently running and accepting connections.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the socket and begins network operations.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the socket and ends network operations.
    /// </summary>
    void Stop();

    /// <summary>
    /// Updates the socket and processes network events.
    /// This method should be called regularly to process pending network events.
    /// </summary>
    void Update();

    /// <summary>
    /// Sends a packet of the specified type to the given network peer with the default delivery method.
    /// Uses the packet type's PacketKey property to identify the packet type.
    /// </summary>
    /// <typeparam name="TPacket">The type of packet to send (must implement IOutgoingPacket).</typeparam>
    /// <param name="peer">The network peer to send the packet to.</param>
    /// <param name="payload">The data payload to include in the packet.</param>
    void SendPacket<TPacket>(NetPeer peer, params object[] payload) where TPacket : Reoria.Engine.Network.Packets.Interfaces.IOutgoingPacket;

    /// <summary>
    /// Sends a packet of the specified type to the given network peer with a specific delivery method.
    /// Uses the packet type's PacketKey property to identify the packet type.
    /// </summary>
    /// <typeparam name="TPacket">The type of packet to send (must implement IOutgoingPacket).</typeparam>
    /// <param name="peer">The network peer to send the packet to.</param>
    /// <param name="deliveryMethod">The delivery method to use for this packet.</param>
    /// <param name="payload">The data payload to include in the packet.</param>
    void SendPacket<TPacket>(NetPeer peer, DeliveryMethod deliveryMethod, params object[] payload) where TPacket : Reoria.Engine.Network.Packets.Interfaces.IOutgoingPacket;
}
