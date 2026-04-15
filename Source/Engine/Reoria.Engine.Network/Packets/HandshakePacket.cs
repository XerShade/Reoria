using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;

namespace Reoria.Server.Network.Packets;

public class HandshakePacket(ILogger<HandshakePacket> logger) : IOutgoingPacket, IIncomingPacket
{
    public string PacketKey
        => "Handshake";

    public string PacketName 
        => "Handshake";

    public string PacketDescription 
        => "Sends a handshake packet to confirm the connection.";

    protected ILogger<HandshakePacket> Logger { get; init; } = logger;

    public NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload)
    {
        Guid randomGuid = Guid.NewGuid();
        writer.Put(randomGuid.ToString());

        return writer;
    }

    public void ReadPacket(NetPeer sender, NetDataReader reader)
    {
        string message = reader.GetString();

        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation("Recieved a handshake packet from {Sender}, the message was: {Message}", sender.Address.ToString(), message);
        }
    }
}