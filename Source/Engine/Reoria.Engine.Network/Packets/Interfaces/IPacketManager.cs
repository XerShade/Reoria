using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;

namespace Reoria.Engine.Network.Packets.Interfaces;

public interface IPacketManager
{
    NetDataWriter ComposeOutgoingPacket(string packetKey, params object[] payload);
    void HandleIncomingPacket(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod);
    void HandleIncomingPacket(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
}