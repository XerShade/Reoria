using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Reoria.Engine.Network.Sockets;

public class ServerSocket : Socket
{
    public ServerSocket(ILogger<ServerSocket> logger)
        : base(logger)
    {

    }

    public override void Start()
        => this.Manager.Start(7234);

    protected override void OnConnectionRequest(ConnectionRequest request)
    {
        this.Logger.LogInformation("Recieved connection request from {Address}.", request.RemoteEndPoint.Address.ToString());

        if (this.Manager.ConnectedPeersCount < 10)
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