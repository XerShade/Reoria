using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Client.Data.Extensions;
using Reoria.Engine.Networking;
using Reoria.Game.Data;
using Reoria.Game.Data.Interfaces;
using SFML.System;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Client.Engine;

public class ClientNetEventListener(IGameData gameData, ILogger<EngineNetEventListener> logger, IConfigurationRoot configuration) : EngineNetEventListener(logger, configuration)
{
    private readonly IGameData gameData = gameData;
    private NetPeer? serverPeer = null;

    public virtual void Start()
    {
        _ = this.netManager.Start();
        this.serverPeer = this.netManager.Connect(this.configuration["Networking:Address"], Convert.ToInt32(this.configuration["Networking:Port"]), this.configuration["Networking:ConnectionKey"]);
    }

    public virtual void Stop() => this.netManager.Stop();

    public override void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        this.logger.LogInformation("A network error has occured: {socketError}", socketError);

        base.OnNetworkError(endPoint, socketError);
    }

    public override void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        string command = reader.GetString().ToUpper();
        this.logger.LogInformation("Received packet {command} from the server.", command);

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

    private void HandleMyId(NetPacketReader reader) => this.gameData.SetLocalPlayerId(reader.GetInt());

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
            this.gameData.Players.Add(player);
        }
    }

    private void HandlePlayerJoined(NetPacketReader reader)
    {
        Player player = new(reader.GetInt(), reader.GetString())
        {
            X = reader.GetFloat(),
            Y = reader.GetFloat()
        };
        this.gameData.Players.Add(player);
    }

    private void HandlePlayerLeft(NetPacketReader reader)
    {
        int playerId = reader.GetInt();
        Player? player = (from p in this.gameData.Players
                          where p.Id.Equals(playerId)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            _ = this.gameData.Players.Remove(player);
        }
    }

    private void HandlePlayerPosition(NetPacketReader reader)
    {
        int playerId = reader.GetInt();
        Player? player = (from p in this.gameData.Players
                          where p.Id.Equals(playerId)
                          select p as Player).FirstOrDefault();

        if (player is not null)
        {
            player.X = reader.GetFloat();
            player.Y = reader.GetFloat();
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
