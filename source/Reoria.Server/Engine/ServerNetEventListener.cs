using LiteNetLib;
using LiteNetLib.Utils;
using Reoria.Engine.Networking;
using Reoria.Game.Data;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Server.Engine;

public class ServerNetEventListener : EngineNetEventListener
{
    private readonly Dictionary<int, Player> players = [];

    public ServerNetEventListener() : base()
    {

    }

    public virtual void Start(int port) => this.netManager.Start(port);
    public virtual void Stop() => this.netManager.Stop();
    public virtual int GetLocalPort() => this.netManager.LocalPort;

    public override void OnPeerConnected(NetPeer peer)
    {
        Console.WriteLine($"Received new client connection from {peer.Address}");

        Random rng = new();
        Player player = new(peer.Id, peer.Address.ToString())
        {
            X = rng.Next(320, 960),
            Y = rng.Next(270, 810)
        };
        this.players.Add(peer.Id, player);

        this.SendPlayerId(peer, player);
        this.SendExistingPlayers(peer, this.players.Values);
        this.SendPlayerJoined(peer, player);

        base.OnPeerConnected(peer);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Console.WriteLine($"Lost connection from {peer.Address}");
        this.SendPlayerLeft(peer);

        if (this.players.ContainsKey(peer.Id))
        {
            _ = this.players.Remove(peer.Id);
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
        _ = request.AcceptIfKey("ReoriaNetworkKey");
        base.OnConnectionRequest(request);
    }

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.WriteLine($"An error has occured on connection from {endPoint}: {socketError}");

        base.OnNetworkError(endPoint, socketError);
    }

    public virtual void BroadcastToAllBut(NetPeer excludePeer, NetDataWriter writer)
    {
        foreach (NetPeer peer in this.netManager.ConnectedPeerList)
        {
            if (peer.Id != excludePeer.Id)
            {
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }

    public virtual void BroadcastToAll(NetDataWriter writer)
    {
        foreach (NetPeer peer in this.netManager.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
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
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
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

        peer.Send(writer, DeliveryMethod.ReliableOrdered);
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
        NetDataWriter writer = new();
        Player player = this.players[peer.Id];
        writer.Put("LEFT");
        writer.Put(player.Id);
        this.BroadcastToAllBut(peer, writer);
    }

    private void HandlePlayerMove(NetPeer peer, NetPacketReader reader)
    {
        Player player = this.players[peer.Id];
        player.X += reader.GetFloat();
        player.Y += reader.GetFloat();
        Console.WriteLine($"Player {player.Id} moved to {player.X}, {player.Y}.");
        this.SendPlayerPosition(player);
    }
}
