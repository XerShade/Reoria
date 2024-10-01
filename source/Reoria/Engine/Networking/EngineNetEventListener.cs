using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Engine.Networking;

public class EngineNetEventListener : INetEventListener
{
    protected readonly NetManager netManager;
    protected readonly ILogger<EngineNetEventListener> logger;
    protected readonly IConfiguration configuration;

    public EngineNetEventListener(ILogger<EngineNetEventListener> logger, IConfigurationRoot configuration)
    {
        this.netManager = new NetManager(this);
        this.logger = logger;
        this.configuration = configuration;
    }

    public virtual void PollEvents() => this.netManager.PollEvents();

    public virtual void OnConnectionRequest(ConnectionRequest request) { }
    public virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
    public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) { }
    public virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public virtual void OnPeerConnected(NetPeer peer) { }
    public virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { }
}
