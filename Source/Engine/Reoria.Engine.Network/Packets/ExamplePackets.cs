using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;

namespace Reoria.Engine.Network.Packets;

/// <summary>
/// Example packet for player login authentication.
/// Demonstrates how to create a packet that accepts username and password parameters.
/// </summary>
/// <param name="logger">The logger instance for packet-related logging operations.</param>
public class PlayerLoginPacket(ILogger<PlayerLoginPacket> logger) : IOutgoingPacket, IIncomingPacket
{
    /// <summary>
    /// Gets the unique key that identifies this packet type.
    /// </summary>
    public string PacketKey => "PlayerLogin";

    /// <summary>
    /// Gets the human-readable name of this packet type.
    /// </summary>
    public string PacketName => "Player Login";

    /// <summary>
    /// Gets a detailed description of what this packet does.
    /// </summary>
    public string PacketDescription => "Authenticates a player with username and password credentials.";

    /// <summary>
    /// Gets the logger instance used for packet-related logging operations.
    /// </summary>
    protected ILogger<PlayerLoginPacket> Logger { get; init; } = logger;

    /// <summary>
    /// Composes the player login packet with username and password.
    /// </summary>
    /// <param name="writer">The data writer to serialize the packet payload into.</param>
    /// <param name="payload">Expected: [username (string), password (string)].</param>
    /// <returns>The NetDataWriter containing the serialized packet data.</returns>
    public NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload)
    {
        if (payload.Length < 2)
        {
            throw new ArgumentException("PlayerLoginPacket requires username and password parameters.");
        }

        string username = payload[0] as string ?? throw new ArgumentException("Username must be a string.");
        string password = payload[1] as string ?? throw new ArgumentException("Password must be a string.");

        writer.Put(username);
        writer.Put(password);

        return writer;
    }

    /// <summary>
    /// Reads and processes an incoming player login packet.
    /// </summary>
    /// <param name="sender">The network peer that sent this packet.</param>
    /// <param name="reader">The data reader containing the packet payload.</param>
    public void ReadPacket(NetPeer sender, NetDataReader reader)
    {
        string username = reader.GetString();
        string password = reader.GetString();

        this.Logger.LogInformation("Received login request from {Address} for user: {Username}", sender.Address.ToString(), username);

        // TODO: Implement authentication logic here
    }
}

/// <summary>
/// Example packet for player login failure response.
/// Demonstrates how to create a response packet with error messages.
/// </summary>
/// <param name="logger">The logger instance for packet-related logging operations.</param>
public class PlayerLoginFailedPacket(ILogger<PlayerLoginFailedPacket> logger) : IOutgoingPacket, IIncomingPacket
{
    /// <summary>
    /// Gets the unique key that identifies this packet type.
    /// </summary>
    public string PacketKey => "PlayerLoginFailed";

    /// <summary>
    /// Gets the human-readable name of this packet type.
    /// </summary>
    public string PacketName => "Player Login Failed";

    /// <summary>
    /// Gets a detailed description of what this packet does.
    /// </summary>
    public string PacketDescription => "Sends a failure response when player login authentication fails.";

    /// <summary>
    /// Gets the logger instance used for packet-related logging operations.
    /// </summary>
    protected ILogger<PlayerLoginFailedPacket> Logger { get; init; } = logger;

    /// <summary>
    /// Composes the player login failed packet with an error message.
    /// </summary>
    /// <param name="writer">The data writer to serialize the packet payload into.</param>
    /// <param name="payload">Expected: [errorMessage (string)].</param>
    /// <returns>The NetDataWriter containing the serialized packet data.</returns>
    public NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload)
    {
        if (payload.Length < 1)
        {
            throw new ArgumentException("PlayerLoginFailedPacket requires an error message parameter.");
        }

        string errorMessage = payload[0] as string ?? throw new ArgumentException("Error message must be a string.");

        writer.Put(errorMessage);

        return writer;
    }

    /// <summary>
    /// Reads and processes an incoming player login failed packet.
    /// </summary>
    /// <param name="sender">The network peer that sent this packet.</param>
    /// <param name="reader">The data reader containing the packet payload.</param>
    public void ReadPacket(NetPeer sender, NetDataReader reader)
    {
        string errorMessage = reader.GetString();

        this.Logger.LogWarning("Login failed from {Address}: {ErrorMessage}", sender.Address.ToString(), errorMessage);

        // TODO: Implement client-side error handling (show error to user, etc.)
    }
}
