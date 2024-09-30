using LiteNetLib;
using LiteNetLib.Utils;
using Reoria.Engine.Networking;
using Reoria.Game.Data;
using SFML.System;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Client.Engine;

public class ClientNetEventListener : EngineNetEventListener
{
    private ClientThread? engineThread;
    private NetPeer? serverPeer = null;

    public virtual ClientNetEventListener AttachToThread(ClientThread engineThread)
    {
        this.engineThread = engineThread;

        return this;
    }

    public virtual void Start(string ipaddress, int port)
    {
        _ = this.netManager.Start();
        this.serverPeer = this.netManager.Connect(ipaddress, port, "ReoriaNetworkKey");
    }

    public virtual void Stop() => this.netManager.Stop();

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Console.WriteLine($"A network error has occured: {socketError}");

        base.OnNetworkError(endPoint, socketError);
    }

    public override void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        string command = reader.GetString().ToUpper();
        Console.WriteLine($"Received packet {command} from the server.");

        switch (command)
        {
            case "ID":
                this.HandleMyId(reader);
                break;
            case "EXISTING_PLAYERS":
                this.HandleExistingPlayers(reader);
                break;
            case "JOINED":
                this.HandlePlayerJoined(reader);
                break;
            case "LEFT":
                this.HandlePlayerLeft(reader);
                break;
            case "POSITION":
                this.HandlePlayerPosition(reader);
                break;
            default:
                break;
        }

        base.OnNetworkReceive(peer, reader, channelNumber, deliveryMethod);
    }

    private void HandleMyId(NetPacketReader reader)
    {
        if(this.engineThread is not null)
        {
            this.engineThread.LocalPlayerId = reader.GetInt();
        }
    }

    private void HandleExistingPlayers(NetPacketReader reader)
    {
        int playercount = reader.GetInt();

        for (int i = 0; i < playercount; i++)
        {
            Player player = new(reader.GetInt(), reader.GetString())
            {
                X = reader.GetFloat(),
                Y = reader.GetFloat()
            };
            this.engineThread?.Players.Add(player.Id, player);
        }
    }

    private void HandlePlayerJoined(NetPacketReader reader)
    {
        Player player = new(reader.GetInt(), reader.GetString())
        {
            X = reader.GetFloat(),
            Y = reader.GetFloat()
        };
        this.engineThread?.Players.Add(player.Id, player);
    }

    private void HandlePlayerLeft(NetPacketReader reader)
    {
        int playerId = reader.GetInt();

        if (this.engineThread is not null)
        {
            if (this.engineThread.Players.ContainsKey(playerId))
            {
                _ = this.engineThread.Players.Remove(playerId);
            }
        }        
    }

    private void HandlePlayerPosition(NetPacketReader reader)
    {
        int playerId = reader.GetInt();
        if (this.engineThread is not null)
        {
            if (this.engineThread.Players.ContainsKey(playerId))
            {
                this.engineThread.Players[playerId].X = reader.GetFloat();
                this.engineThread.Players[playerId].Y = reader.GetFloat();
            }
        }
    }

    public virtual void SendMoveCommand(Vector2f velocity)
    {
        NetDataWriter writer = new();
        writer.Put("MOVE");
        writer.Put(velocity.X);
        writer.Put(velocity.Y);
        this.serverPeer?.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}
