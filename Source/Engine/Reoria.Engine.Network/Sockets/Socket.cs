using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Engine.Network.Sockets;

public abstract class Socket : IDisposable
{
    protected virtual ILogger<Socket> Logger { get; init; }
    protected virtual EventBasedNetListener Listener { get; init; }
    protected virtual NetManager Manager { get; init; }
    protected virtual string ConnectionKey { get; init; }
    protected virtual int Port { get; init; }

    public virtual bool IsRunning => this.Manager.IsRunning;

    protected Socket(ILogger<Socket> logger, IConfiguration configuration)
    {
        this.Logger = logger;
        this.Listener = new EventBasedNetListener();
        this.Manager = new NetManager(this.Listener);

        this.ConnectionKey = configuration["Networking:ConnectionKey"] ?? "Reoria";
        this.Port = Convert.ToInt32(configuration["Networking:Port"] ?? "7234");

        this.AttachEvents();
    }

    protected virtual void AttachEvents()
    {
        this.Listener.ConnectionRequestEvent += this.OnConnectionRequest;
        this.Listener.NetworkErrorEvent += this.OnNetworkError;
        this.Listener.NetworkReceiveEvent += this.OnNetworkReceive;
        this.Listener.NetworkReceiveUnconnectedEvent += this.OnNetworkReceiveUnconnected;
        this.Listener.PeerAddressChangedEvent += this.OnPeerAddressChanged;
        this.Listener.PeerConnectedEvent += this.OnPeerConnected;
        this.Listener.PeerDisconnectedEvent += this.OnPeerDisconnected;
    }

    protected virtual void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.LogWarning("Recieved connection request from {Address}, denying it. This socket is not a host.", request.RemoteEndPoint.Address.ToString());
        request.Reject();
    }

    protected virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        => this.Logger.LogError("Network error from endpoint {EndPoint}: {SocketError}", endPoint.Address.ToString(), socketError);

    protected virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        => this.Logger.LogInformation("Recieved message from peer {Address}.", peer.Address.ToString());

    protected virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        => this.Logger.LogInformation("Recieved unconnected message from {Address}.", remoteEndPoint.Address.ToString());

    protected virtual void OnPeerAddressChanged(NetPeer peer, IPEndPoint previousAddress)
        => this.Logger.LogWarning("Peer address has changed from {PreviousAddress} to {NewAddress}.", previousAddress.Address.ToString(), peer.Address.ToString());

    protected virtual void OnPeerConnected(NetPeer peer)
        => this.Logger.LogInformation("Peer {Address} has connected.", peer.Address.ToString());

    protected virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        => this.Logger.LogInformation("Peer {Address} has disconnected, reason: {Reason}.", peer.Address.ToString(), disconnectInfo.Reason);

    public virtual void Start()
        => this.Manager.Start();

    public virtual void Stop()
        => this.Manager.Stop();

    public virtual void Update()
        => this.Manager.PollEvents();

    public void Dispose()
        => GC.SuppressFinalize(this);

    public virtual bool Connect(string address, int port, int timeout = 5)
    {
        this.Logger.LogError("Unable to connect to host {Host}:{Port}, this socket does not have outgoing connectivity support.", address, port);

        return false;
    }
}