using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Reoria.Engine;
using Reoria.Game.DbContexts;

namespace Reoria.Server.Data.DbContexts;

public class SqliteDbContext : GameDbContext
{
    private readonly IConfigurationRoot configuration;

    public SqliteDbContext() => this.configuration = new EngineServiceProvider().Configuration;
    public SqliteDbContext(IConfigurationRoot configuration) => this.configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSqlite(this.configuration.GetConnectionString("GameDataConnectionString"));

        base.OnConfiguring(optionsBuilder);
    }
}
