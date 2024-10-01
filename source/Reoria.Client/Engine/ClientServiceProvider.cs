using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;

namespace Reoria.Client.Engine;

public class ClientServiceProvider : EngineServiceProvider
{
    public ClientServiceProvider() : base()
    {
        _ = this.serviceCollection.AddSingleton<ClientShared>();
        _ = this.serviceCollection.AddSingleton<ClientNetEventListener>();
        _ = this.serviceCollection.AddSingleton<ClientThread>();
        _ = this.serviceCollection.AddSingleton<ClientRenderWindow>();
    }
}
