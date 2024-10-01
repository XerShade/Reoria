using Reoria.Game.Data;

namespace Reoria.Client.Engine;

public class ClientShared
{
    public int LocalPlayerId;
    public readonly Dictionary<int, Player> Players = [];
}
