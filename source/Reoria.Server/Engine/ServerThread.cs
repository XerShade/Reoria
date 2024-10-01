using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine;

namespace Reoria.Server.Engine;

public class ServerThread : EngineThread
{
    private readonly ServerNetEventListener networking = new();

    public ServerThread(ServiceProvider serviceProvider, int ticksPerSecond = 60) : base(serviceProvider, ticksPerSecond)
    {
    }

    protected override void OnThreadStart()
    {
        this.networking.Start(5555);
        this.logger.LogInformation("Started server on {Port}.", this.networking.GetLocalPort());

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.networking.Stop();
        this.logger.LogInformation("Stopped listening on {Port}", this.networking.GetLocalPort());

        base.OnThreadStop();
    }

    protected override void OnThreadTick()
    {
        this.networking.PollEvents();

        base.OnThreadTick();
    }
}
