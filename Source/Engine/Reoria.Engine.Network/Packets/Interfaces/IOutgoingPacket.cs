using LiteNetLib.Utils;

namespace Reoria.Engine.Network.Packets.Interfaces;

public interface IOutgoingPacket
{
    string PacketKey { get; }
    string PacketName { get; }
    string PacketDescription { get; }
    NetDataWriter ComposePacket(NetDataWriter writer, params object[] payload);
}