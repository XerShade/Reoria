using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;

namespace Reoria.Engine.Network.Packets;

/// <summary>
/// Implements a handshake packet used to establish and confirm network connections between clients and server.
/// This packet serves as an initial communication step to verify connectivity and establish a unique session identifier.
/// </summary>
/// <param name="logger">The logger instance for handshake-related logging operations.</param>
public class HandshakePacket(ILogger<HandshakePacket> logger) : IOutgoingPacket, IIncomingPacket
{
    /// <summary>
    /// Gets the unique key that identifies this packet type.
    /// Must match between client and server for proper packet routing.
    /// </summary>
    public string PacketKey
        => "Handshake";

    /// <summary>
    /// Gets the human-readable name of this packet type.
    /// Used for logging, debugging, and UI display purposes.
    /// </summary>
    public string PacketName
        => "Handshake";

    /// <summary>
    /// Gets a detailed description of what this packet does and when it's used.
    /// This packet is used to confirm network connectivity and establish session context.
    /// </summary>
    public string PacketDescription
        => "Sends a handshake packet to confirm the connection and establish session context.";

    /// <summary>
    /// Gets the logger instance used for handshake-related logging operations.
    /// </summary>
    protected ILogger<HandshakePacket> Logger { get; init; } = logger;

    /// <summary>
    /// Composes the handshake packet data for transmission.
    /// Generates a unique GUID to serve as a session identifier for this connection.
    /// </summary>
    /// <param name="writer">The data writer to serialize the packet payload into.</param>
    /// <param name="payload">Optional payload data (not used for handshake packets).</param>
    /// <returns>The NetDataWriter containing the serialized handshake packet data.</returns>
    public NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload)
    {
        // Generate a unique GUID to serve as a session identifier.
        Guid sessionGuid = Guid.NewGuid();
        writer.Put(sessionGuid.ToString());

        return writer;
    }

    /// <summary>
    /// Reads and processes an incoming handshake packet from a network peer.
    /// Logs the received handshake information for connection verification.
    /// </summary>
    /// <param name="sender">The network peer that sent this handshake packet.</param>
    /// <param name="reader">The data reader containing the handshake packet payload.</param>
    public void ReadPacket(NetPeer sender, NetDataReader reader)
    {
        // Extract the session identifier from the packet payload.
        string sessionMessage = reader.GetString();

        // Log the handshake receipt for debugging and connection tracking.
        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation("Received a handshake packet from {Sender}, the session identifier was: {Message}",
                sender.Address.ToString(), sessionMessage);
        }
    }
}