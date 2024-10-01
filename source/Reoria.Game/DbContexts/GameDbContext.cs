using Microsoft.EntityFrameworkCore;
using Reoria.Game.Data;

namespace Reoria.Game.DbContexts;

public abstract class GameDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
}
