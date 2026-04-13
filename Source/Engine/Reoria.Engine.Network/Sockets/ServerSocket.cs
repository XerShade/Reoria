using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Reoria.Engine.Network.Sockets;

public class ServerSocket : Socket
{
    protected virtual int MaxConnections { get; init; }

    public ServerSocket(ILogger<ServerSocket> logger, IConfiguration configuration)
        : base(logger, configuration) 
        => this.MaxConnections = Convert.ToInt32(configuration["Networking:MaxConnections"] ?? "10");

    public override void Start()
        => this.Manager.Start(this.Port);

    protected override void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.LogInformation("Recieved connection request from {Address}.", request.RemoteEndPoint.Address.ToString());

        if (this.Manager.ConnectedPeersCount < this.MaxConnections)
        {
            NetPeer peer = request.AcceptIfKey("Reoria");

            if(peer != null)
            {
                this.Logger.LogInformation("Connection request from {Address} was accepted.", request.RemoteEndPoint.Address.ToString());
            }
            else
            {
                this.Logger.LogWarning("Connection request from {Address} was rejected.", request.RemoteEndPoint.Address.ToString());
            }
        }
        else
        {
            this.Logger.LogWarning("Connection request from {Address} was rejected, the server is full.", request.RemoteEndPoint.Address.ToString());
            request.Reject();
        }
    }
}