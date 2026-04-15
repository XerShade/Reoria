using LiteNetLib;
using LiteNetLib.Utils;

namespace Reoria.Engine.Network.Packets.Interfaces;

public interface IIncomingPacket
{
    string PacketKey { get; }
    string PacketName { get; }
    string PacketDescription { get; }
    void ReadPacket(NetPeer sender, NetDataReader reader);
}