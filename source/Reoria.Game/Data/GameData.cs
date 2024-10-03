using Reoria.Game.Data.Interfaces;
using Reoria.Game.DbContexts;

namespace Reoria.Game.Data;

public class GameData(GameDbContext dbContext) : IGameData
{
    private readonly GameDbContext dbContext = dbContext;
    public List<Player> Players { get; protected set; } = [];
}
