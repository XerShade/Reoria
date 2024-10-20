using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine;

namespace Reoria.Server.Engine;

public class ServerThread(IServiceProvider services, int ticksPerSecond = 60) : EngineThread(services, ticksPerSecond)
{
    private readonly ServerNetEventListener networking = services.GetRequiredService<ServerNetEventListener>();

    protected override void OnThreadStart()
    {
        this.networking.Start();
        this.logger.LogInformation("Started server on {Port}.", this.networking.GetLocalPort());

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.networking.Stop();
        this.logger.LogInformation("Stopped listening on {Port}", this.networking.GetLocalPort());

        base.OnThreadStop();
    }

    protected override void OnThreadDynamicTick(float deltaTime)
    {
        this.networking.PollEvents();

        base.OnThreadDynamicTick(deltaTime);
    }
}