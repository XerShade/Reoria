using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine;

namespace Reoria.Server.Engine;

public class ServerServiceProvider : EngineServiceProvider
{
    public ServerServiceProvider() : base() => this.serviceCollection.AddSingleton<ServerNetEventListener>();
}
