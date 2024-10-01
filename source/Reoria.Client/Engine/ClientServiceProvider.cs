using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;
using Reoria.Game.DbContexts;
using Reoria.Game.State;
using Reoria.Game.State.Interfaces;

namespace Reoria.Client.Engine;

public class ClientServiceProvider : EngineServiceProvider
{
    public ClientServiceProvider() : base()
    {
        _ = this.serviceCollection.AddDbContext<GameDbContext, MemoryDbContext>();
        _ = this.serviceCollection.AddSingleton<IGameState, GameState>();
        _ = this.serviceCollection.AddSingleton<ClientNetEventListener>();
        _ = this.serviceCollection.AddSingleton<ClientRenderWindow>();
    }
}
