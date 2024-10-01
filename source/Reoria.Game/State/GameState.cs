using Reoria.Game.Data;
using Reoria.Game.DbContexts;
using Reoria.Game.State.Interfaces;

namespace Reoria.Game.State;

public class GameState(GameDbContext dbContext) : IGameState
{
    private readonly GameDbContext dbContext = dbContext;
    public List<Player> Players { get; protected set; } = [];
    public int LocalPlayerId { get; set; } = 0;
}
