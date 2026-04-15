using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;
using System.Net;

namespace Reoria.Engine.Network.Packets;

public class PacketManager(ILogger<IPacketManager> logger, IEnumerable<IIncomingPacket> incomingPackets, IEnumerable<IOutgoingPacket> outgoingPackets) : IPacketManager
{
    protected virtual ILogger<IPacketManager> Logger { get; init; } = logger;
    protected virtual List<IIncomingPacket> IncomingPackets { get; init; } = [.. incomingPackets];
    protected virtual List<IOutgoingPacket> OutgoingPackets { get; init; } = [.. outgoingPackets];

    public virtual void HandleIncomingPacket(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        // Read packet metadata first.
        string packetKey = reader.GetString();

        // Attempt to get the packet handler.
        IIncomingPacket? incomingPacket = this.IncomingPackets.FirstOrDefault(x => x.PacketKey == packetKey);

        // Handle the packet, or throw an exception if no handler was found.
        incomingPacket?.ReadPacket(peer, reader);
    }

    public void HandleIncomingPacket(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        => this.Logger.LogWarning("Recieved an unconnected packet of size {Size} from {Address} with type {Type}, this is not supported.",
            reader.RawDataSize, remoteEndPoint.ToString(), messageType.ToString());        

    public virtual NetDataWriter ComposeOutgoingPacket(string packetKey, params object[] payload)
    {
        // Create a new NetDataWriter for the packet.
        NetDataWriter writer = new();

        // Write packet metadata first.
        writer.Put(packetKey);

        // Attempt to get the packet composer with the specified key.
        IOutgoingPacket? packet = this.OutgoingPackets.FirstOrDefault(x => x.PacketKey == packetKey);

        // Compose the packet, or throw an exception if no composer was found.
        return packet != null
            ? packet.ComposePacket(writer, payload)
            : throw new InvalidOperationException("Unable to compose packet with key: " + packetKey);
    }
}