using Reoria.Game.Data;

namespace Reoria.Game.State.Interfaces;

public interface IGameState
{
    public List<Player> Players { get; }
    int LocalPlayerId { get; set; }
}
