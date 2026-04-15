using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Packets.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Engine.Network.Sockets;

/// <summary>
/// Abstract base class for network socket functionality using LiteNetLib.
/// </summary>
/// <remarks>
/// This class provides the foundation for both client and server network implementations.
/// It handles common functionality like event management, configuration, and basic
/// networking operations. Derived classes should implement specific client or server behavior.
/// </remarks>
public abstract class Socket : IDisposable
{
    /// <summary>
    /// Gets the logger instance for this socket.
    /// </summary>
    protected virtual ILogger<Socket> Logger { get; init; }
    
    /// <summary>
    /// Gets the LiteNetLib event listener for this socket.
    /// </summary>
    protected virtual EventBasedNetListener Listener { get; init; }
    
    /// <summary>
    /// Gets the LiteNetLib network manager for this socket.
    /// </summary>
    protected virtual NetManager Manager { get; init; }

    /// <summary>
    /// Gets the packet manager for this socket.
    /// </summary>
    protected virtual IPacketManager PacketManager { get; init; }
    
    /// <summary>
    /// Gets the connection key used for authentication.
    /// </summary>
    /// <remarks>
    /// This key is used to validate connection requests between peers.
    /// Default value is "Reoria" but can be overridden in configuration.
    /// </remarks>
    protected virtual string ConnectionKey { get; init; }
    
    /// <summary>
    /// Gets the network port this socket operates on.
    /// </summary>
    protected virtual int Port { get; init; }

    /// <summary>
    /// Gets whether the socket is currently running and accepting connections.
    /// </summary>
    /// <remarks>
    /// For servers, this indicates the server is listening for connections.
    /// For clients, this typically indicates connection status.
    /// </remarks>
    public virtual bool IsRunning => this.Manager.IsRunning;

    /// <summary>
    /// Initializes a new instance of the Socket class.
    /// </summary>
    /// <param name="logger">The logger instance for this socket.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <remarks>
    /// This constructor sets up the basic networking infrastructure including:
    /// - Event listener for handling network events
    /// - Network manager for connection management
    /// - Configuration values for connection key and port
    /// - Event handlers for all network operations
    /// </remarks>
    protected Socket(ILogger<Socket> logger, IConfiguration configuration, IPacketManager packetManager)
    {
        this.Logger = logger;
        this.Listener = new EventBasedNetListener();
        this.Manager = new NetManager(this.Listener);
        this.PacketManager = packetManager;

        // Load configuration values with defaults.
        this.ConnectionKey = configuration["Networking:ConnectionKey"] ?? "Reoria";
        this.Port = Convert.ToInt32(configuration["Networking:Port"] ?? "7234");

        // Attach event handlers for network operations.
        this.AttachEvents();
    }

    /// <summary>
    /// Attaches event handlers to LiteNetLib events.
    /// </summary>
    /// <remarks>
    /// This method sets up all the necessary event handlers for monitoring
    /// network activities like connection requests, errors, message reception,
    /// and peer state changes. Derived classes can override individual handlers
    /// to implement custom behavior.
    /// </remarks>
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

    /// <summary>
    /// Called when a connection request is received.
    /// </summary>
    /// <param name="request">The connection request from a remote peer.</param>
    /// <remarks>
    /// Base implementation denies all connection requests since this is an abstract socket.
    /// Derived classes should override this method to implement specific connection logic.
    /// </remarks>
    protected virtual void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.LogWarning("Received connection request from {Address}, denying it. This socket is not a host.", request.RemoteEndPoint.Address.ToString());
        request.Reject();
    }

    /// <summary>
    /// Called when a network error occurs.
    /// </summary>
    /// <param name="endPoint">The endpoint where the error occurred.</param>
    /// <param name="socketError">The type of network error.</param>
    /// <remarks>
    /// Base implementation logs all network errors. Derived classes can override
    /// to implement custom error handling or recovery logic.
    /// </remarks>
    protected virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        => this.Logger.LogError("Network error from endpoint {EndPoint}: {SocketError}", endPoint.Address.ToString(), socketError);

    /// <summary>
    /// Called when a network message is received from a connected peer.
    /// </summary>
    /// <param name="peer">The peer that sent the message.</param>
    /// <param name="reader">The packet reader containing the message data.</param>
    /// <param name="channel">The channel the message was received on.</param>
    /// <param name="deliveryMethod">The delivery method used for the message.</param>
    /// <remarks>
    /// Base implementation logs all received messages. Derived classes should override
    /// to implement custom message processing and handling logic.
    /// </remarks>
    protected virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        this.Logger.LogInformation("Received packet from peer {Address}.", peer.Address.ToString());

        this.PacketManager.HandleIncomingPacket(peer, reader, channel, deliveryMethod);
    }

    /// <summary>
    /// Called when an unconnected message is received.
    /// </summary>
    /// <param name="remoteEndPoint">The endpoint that sent the message.</param>
    /// <param name="reader">The packet reader containing the message data.</param>
    /// <param name="messageType">The type of unconnected message.</param>
    /// <remarks>
    /// Base implementation logs all unconnected messages. Derived classes should override
    /// to implement custom handling for connectionless messages like discovery broadcasts.
    /// </remarks>
    protected virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        this.Logger.LogInformation("Received unconnected message from {Address}.", remoteEndPoint.Address.ToString());

        this.PacketManager.HandleIncomingPacket(remoteEndPoint, reader, messageType);
    }

    /// <summary>
    /// Called when a peer's address changes.
    /// </summary>
    /// <param name="peer">The peer whose address changed.</param>
    /// <param name="previousAddress">The previous address of the peer.</param>
    /// <remarks>
    /// Base implementation logs address changes. This can happen during
    /// network reconnections or address reassignments. Derived classes can override
    /// to implement custom address change handling.
    /// </remarks>
    protected virtual void OnPeerAddressChanged(NetPeer peer, IPEndPoint previousAddress)
        => this.Logger.LogWarning("Peer address has changed from {PreviousAddress} to {NewAddress}.", previousAddress.Address.ToString(), peer.Address.ToString());

    /// <summary>
    /// Called when a peer successfully connects.
    /// </summary>
    /// <param name="peer">The peer that connected.</param>
    /// <remarks>
    /// Base implementation logs successful connections. Derived classes can override
    /// to implement custom connection handling, initialization, or notification logic.
    /// </remarks>
    protected virtual void OnPeerConnected(NetPeer peer)
    {
        this.Logger.LogInformation("Peer {Address} has connected.", peer.Address.ToString());

        NetDataWriter writer = this.PacketManager.ComposeOutgoingPacket("Handshake");

        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// Called when a peer disconnects.
    /// </summary>
    /// <param name="peer">The peer that disconnected.</param>
    /// <param name="disconnectInfo">Information about the disconnection reason.</param>
    /// <remarks>
    /// Base implementation logs disconnection details. Derived classes can override
    /// to implement custom cleanup, reconnection logic, or disconnection handling.
    /// </remarks>
    protected virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        => this.Logger.LogInformation("Peer {Address} has disconnected, reason: {Reason}.", peer.Address.ToString(), disconnectInfo.Reason);

    /// <summary>
    /// Starts the socket and begins network operations.
    /// </summary>
    /// <remarks>
    /// Base implementation starts the LiteNetLib manager.
    /// Derived classes should override to implement custom startup logic.
    /// </remarks>
    public virtual void Start()
        => this.Manager.Start();

    /// <summary>
    /// Stops the socket and ends network operations.
    /// </summary>
    /// <remarks>
    /// Base implementation stops the LiteNetLib manager.
    /// Derived classes should override to implement custom shutdown logic.
    /// </remarks>
    public virtual void Stop()
        => this.Manager.Stop();

    /// <summary>
    /// Updates the socket and processes network events.
    /// </summary>
    /// <remarks>
    /// This method should be called regularly to process pending network events.
    /// Base implementation polls LiteNetLib events. Derived classes should override
    /// to implement custom update logic or event processing.
    /// </remarks>
    public virtual void Update()
        => this.Manager.PollEvents();

    /// <summary>
    /// Releases all resources used by the socket.
    /// </summary>
    /// <remarks>
    /// Base implementation suppresses finalization and calls Stop().
    /// Derived classes should override to implement custom cleanup logic.
    /// </remarks>
    public void Dispose()
    {
        this.Stop();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Attempts to connect to a remote host (not supported for base socket).
    /// </summary>
    /// <param name="address">The host address to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="timeout">Connection timeout in seconds.</param>
    /// <returns>Always returns false as base socket doesn't support outgoing connections.</returns>
    /// <remarks>
    /// Base socket class doesn't implement client connectivity.
    /// Use ClientSocket class for client connections or override in derived classes.
    /// </remarks>
    public virtual bool Connect(string address, int port, int timeout = 5)
    {
        this.Logger.LogError("Unable to connect to host {Host}:{Port}, this socket does not have outgoing connectivity support.", address, port);

        return false;
    }
}