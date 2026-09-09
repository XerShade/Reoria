using LiteNetLib;
using LiteNetLib.Utils;
using Reoria.Engine.Network.Packets;
using Reoria.Engine.Network.Sockets;

namespace Reoria.Engine.Network.Examples;

/// <summary>
/// Example demonstrating how to use the new type-safe packet sending system.
/// This shows how to send packets using the generic SendPacket method.
/// </summary>
public class PacketSendingExample
{
    /// <summary>
    /// Example of sending a player login packet from client to server.
    /// Uses the default delivery method (ReliableOrdered).
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="serverPeer">The server peer to send the packet to.</param>
    public void SendPlayerLogin(INetworkSocket socket, NetPeer serverPeer)
    {
        // Send a PlayerLogin packet with username and password
        // The system automatically uses the PacketKey from PlayerLoginPacket
        socket.SendPacket<PlayerLoginPacket>(serverPeer, "MyUsername", "MyPassword");
    }

    /// <summary>
    /// Example of sending a player login packet with a specific delivery method.
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="serverPeer">The server peer to send the packet to.</param>
    public void SendPlayerLoginWithDeliveryMethod(INetworkSocket socket, NetPeer serverPeer)
    {
        // Send a PlayerLogin packet with explicit delivery method
        socket.SendPacket<PlayerLoginPacket>(serverPeer, DeliveryMethod.ReliableOrdered, "MyUsername", "MyPassword");
    }

    /// <summary>
    /// Example of sending a login failure response from server to client.
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="clientPeer">The client peer to send the packet to.</param>
    public void SendLoginFailure(INetworkSocket socket, NetPeer clientPeer)
    {
        // Send a PlayerLoginFailed packet with error message
        socket.SendPacket<PlayerLoginFailedPacket>(clientPeer, "Unknown username or password.");
    }

    /// <summary>
    /// Example of sending a login failure response with a specific delivery method.
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="clientPeer">The client peer to send the packet to.</param>
    public void SendLoginFailureWithDeliveryMethod(INetworkSocket socket, NetPeer clientPeer)
    {
        // Send a PlayerLoginFailed packet with explicit delivery method
        socket.SendPacket<PlayerLoginFailedPacket>(clientPeer, DeliveryMethod.ReliableOrdered, "Unknown username or password.");
    }

    /// <summary>
    /// Example showing how to send the existing Handshake packet using the new system.
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="peer">The peer to send the handshake to.</param>
    public void SendHandshake(INetworkSocket socket, NetPeer peer)
    {
        // Send a Handshake packet (no payload needed for this packet type)
        socket.SendPacket<HandshakePacket>(peer);
    }

    /// <summary>
    /// Example showing different delivery methods for different packet types.
    /// </summary>
    /// <param name="socket">The network socket instance.</param>
    /// <param name="peer">The peer to send packets to.</param>
    public void SendPacketsWithDifferentDeliveryMethods(INetworkSocket socket, NetPeer peer)
    {
        // Send authentication packet with reliable ordered delivery
        socket.SendPacket<PlayerLoginPacket>(peer, DeliveryMethod.ReliableOrdered, "user", "pass");

        // Send a failure response with reliable ordered delivery
        socket.SendPacket<PlayerLoginFailedPacket>(peer, DeliveryMethod.ReliableOrdered, "Invalid credentials");

        // Send a heartbeat packet with unreliable delivery (if such a packet existed)
        // socket.SendPacket<HeartbeatPacket>(peer, DeliveryMethod.Unreliable);
    }
}
