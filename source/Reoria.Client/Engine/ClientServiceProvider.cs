using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;
using Reoria.Game.Data;
using Reoria.Game.Data.Interfaces;
using Reoria.Game.DbContexts;

namespace Reoria.Client.Engine;

public class ClientServiceProvider : EngineServiceProvider
{
    public ClientServiceProvider() : base()
    {
        _ = this.serviceCollection.AddDbContext<GameDbContext, MemoryDbContext>();
        _ = this.serviceCollection.AddSingleton<IGameData, GameData>();
        _ = this.serviceCollection.AddSingleton<ClientNetEventListener>();
        _ = this.serviceCollection.AddSingleton<ClientRenderWindow>();
    }
}
