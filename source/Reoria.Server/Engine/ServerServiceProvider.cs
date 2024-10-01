using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;
using Reoria.Game.DbContexts;
using Reoria.Server.Data.DbContexts;

namespace Reoria.Server.Engine;

public class ServerServiceProvider : EngineServiceProvider
{
    public ServerServiceProvider() : base()
    {
        _ = this.serviceCollection.AddSingleton<ServerNetEventListener>();
        _ = this.serviceCollection.AddDbContext<GameDbContext, SqliteDbContext>();
    }
}
