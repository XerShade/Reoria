using LiteNetLib;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Engine.Networking;

public class EngineNetEventListener : INetEventListener
{
    protected readonly NetManager netManager;

    public EngineNetEventListener() => this.netManager = new NetManager(this);

    public virtual void PollEvents() => this.netManager.PollEvents();

    public virtual void OnConnectionRequest(ConnectionRequest request) { }
    public virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
    public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) { }
    public virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public virtual void OnPeerConnected(NetPeer peer) { }
    public virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }
}
