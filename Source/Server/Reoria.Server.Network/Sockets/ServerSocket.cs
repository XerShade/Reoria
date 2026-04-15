using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Sockets;
using Reoria.Server.Network.Sessions;
using Reoria.Server.Network.Sessions.Interfaces;

namespace Reoria.Server.Network.Sockets;

/// <summary>
/// Represents a server-side network socket for accepting client connections.
/// </summary>
/// <remarks>
/// This class provides server networking functionality using LiteNetLib, with support for
/// configurable maximum connections, connection request handling, and comprehensive logging.
/// It inherits from the base Socket class and implements server-specific connection logic.
/// </remarks>
/// <inheritdoc />
public class ServerSocket(ILogger<ServerSocket> logger, IConfiguration configuration, ISessionManager sessionManager) : Socket(logger, configuration)
{
    /// <summary>
    /// Gets the maximum number of concurrent connections allowed.
    /// </summary>
    /// <remarks>
    /// Defaults to 10 if not specified in configuration.
    /// This limit helps prevent server overload and manages resource usage.
    /// </remarks>
    protected virtual int MaxConnections { get; init; } = Convert.ToInt32(configuration["Networking:MaxConnections"] ?? "10");

    /// <summary>
    /// Gets the session manager associated with the server socket.
    /// </summary>
    protected virtual ISessionManager SessionManager { get;init; } = sessionManager;

    /// <summary>
    /// Starts the server and begins listening for client connections.
    /// </summary>
    /// <remarks>
    /// This method starts the LiteNetLib server on the configured port.
    /// The server will begin accepting connection requests immediately.
    /// </remarks>
    public override void Start()
        => this.Manager.Start(this.Port);

    /// <summary>
    /// Called when a client requests a connection to the server.
    /// </summary>
    /// <param name="request">The connection request from the client.</param>
    /// <remarks>
    /// This method handles incoming connection requests, checking if the server
    /// has capacity for more connections and validates the request before accepting.
    /// </remarks>
    protected override void OnConnectionRequest(ConnectionRequest request)
    {
        // Log the incoming connection request for monitoring.
        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation("Received connection request from {Address}.", request.RemoteEndPoint.Address.ToString());
        }

        // Check if server has capacity for more connections.
        if (this.Manager.ConnectedPeersCount < this.MaxConnections)
        {
            // Accept the connection if it has the proper authentication key.
            NetPeer peer = request.AcceptIfKey("Reoria");

            if (peer != null)
            {
                // Open the session for the accepted peer.
                Session session = this.SessionManager.Open(peer);

                // Log successful connection acceptance.
                if (this.Logger.IsEnabled(LogLevel.Information))
                {
                    this.Logger.LogInformation("Connection request from {Address} was accepted, session {SessionId} opened.", request.RemoteEndPoint.Address.ToString(), session.Guid);
                }
            }
            else
            {
                // Log connection rejection due to missing authentication key.
                this.Logger.LogWarning("Connection request from {Address} was rejected.", request.RemoteEndPoint.Address.ToString());
            }
        }
        else
        {
            // Reject connection due to server being at maximum capacity.
            this.Logger.LogWarning("Connection request from {Address} was rejected, server is full.", request.RemoteEndPoint.Address.ToString());
            request.Reject();
        }
    }

    /// <summary>
    /// Handles the disconnection of a client from the server.
    /// </summary>
    /// <param name="peer">The peer that has disconnected.</param>
    /// <param name="disconnectInfo">The reason for the disconnection.</param>
    protected override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // Close the session for the disconnected peer.
        this.SessionManager.Close(peer);

        // Call the base method to handle the disconnection.
        base.OnPeerDisconnected(peer, disconnectInfo);
    }
}