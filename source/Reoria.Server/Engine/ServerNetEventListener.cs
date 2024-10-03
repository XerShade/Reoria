using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Networking;
using Reoria.Game.Data;
using Reoria.Game.Data.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Server.Engine;

public class ServerNetEventListener(IGameData gameData, ILogger<EngineNetEventListener> logger, IConfigurationRoot configuration) : EngineNetEventListener(logger, configuration)
{
    private readonly IGameData gameData = gameData;

    public virtual void Start() => this.netManager.Start(Convert.ToInt32(this.configuration["Networking:Port"]));
    public virtual void Stop() => this.netManager.Stop();
    public virtual int GetLocalPort() => this.netManager.LocalPort;

    public override void OnPeerConnected(NetPeer peer)
    {
        this.logger.LogInformation("Received new client connection from {Address}.", peer.Address);

        Random rng = new();
        Player player = new(peer.Id, peer.Address.ToString())
        {
            X = rng.Next(320, 960),
            Y = rng.Next(270, 810)
        };
        this.gameData.Players.Add(player);

        this.SendPlayerId(peer, player);
        this.SendExistingPlayers(peer, this.gameData.Players);
        this.SendPlayerJoined(peer, player);

        base.OnPeerConnected(peer);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        this.logger.LogInformation("Lost connection from {Address}", peer.Address);
        this.SendPlayerLeft(peer);

        Player? player = (from p in this.gameData.Players
                          where p.Id.Equals(peer.Id)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            _ = this.gameData.Players.Remove(player);
        }

        base.OnPeerDisconnected(peer, disconnectInfo);
    }

    public override void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        string command = reader.GetString().ToUpper();

        switch (command)
        {
            case "MOVE":
                this.HandlePlayerMove(peer, reader);
                break;
            default:
                break;
        }

        base.OnNetworkReceive(peer, reader, channelNumber, deliveryMethod);
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        _ = request.AcceptIfKey(this.configuration["Networking:ConnectionKey"]);
        base.OnConnectionRequest(request);
    }

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        this.logger.LogInformation("An error has occured on connection from {endPoint}: {socketError}", endPoint, socketError);

        base.OnNetworkError(endPoint, socketError);
    }

    public virtual void BroadcastTo(NetPeer peer, NetDataWriter writer)
    {
        if (this.netManager.IsRunning)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public virtual void BroadcastToAllBut(NetPeer excludePeer, NetDataWriter writer)
    {
        foreach (NetPeer peer in this.netManager.ConnectedPeerList)
        {
            if (peer.Id != excludePeer.Id)
            {
                this.BroadcastTo(peer, writer);
            }
        }
    }

    public virtual void BroadcastToAll(NetDataWriter writer)
    {
        foreach (NetPeer peer in this.netManager.ConnectedPeerList)
        {
            this.BroadcastTo(peer, writer);
        }
    }

    private void SendPlayerPosition(Player player)
    {
        NetDataWriter writer = new();
        writer.Put("POSITION");
        writer.Put(player.Id);
        writer.Put(player.X);
        writer.Put(player.Y);
        this.BroadcastToAll(writer);
    }

    private void SendPlayerId(NetPeer peer, Player player)
    {
        NetDataWriter writer = new();
        writer.Put("ID");
        writer.Put(player.Id);
        this.BroadcastTo(peer, writer);
    }

    private void SendExistingPlayers(NetPeer peer, IEnumerable<Player> players)
    {
        NetDataWriter writer = new();
        writer.Put("EXISTING_PLAYERS");
        writer.Put(players.Count());

        foreach (Player player in players)
        {
            writer.Put(player.Id);
            writer.Put(player.Name);
            writer.Put(player.X);
            writer.Put(player.Y);
        }

        this.BroadcastTo(peer, writer);
    }

    private void SendPlayerJoined(NetPeer peer, Player player)
    {
        NetDataWriter writer = new();
        writer.Put("JOINED");
        writer.Put(player.Id);
        writer.Put(player.Name);
        writer.Put(player.X);
        writer.Put(player.Y);
        this.BroadcastToAllBut(peer, writer);
    }

    private void SendPlayerLeft(NetPeer peer)
    {
        Player? player = (from p in this.gameData.Players
                          where p.Id.Equals(peer.Id)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            NetDataWriter writer = new();
            writer.Put("LEFT");
            writer.Put(player.Id);
            this.BroadcastToAllBut(peer, writer);
        }      
    }

    private void HandlePlayerMove(NetPeer peer, NetPacketReader reader)
    {
        Player? player = (from p in this.gameData.Players
                          where p.Id.Equals(peer.Id)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            player.X += reader.GetFloat();
            player.Y += reader.GetFloat();
            this.logger.LogInformation("Player {Id} moved to {X}, {Y}.", player.Id, player.X, player.Y);
            this.SendPlayerPosition(player);
        }
    }
}
