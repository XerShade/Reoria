using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine;

namespace Reoria.Server.Engine;

public class ServerThread(ServerNetEventListener serverNetEventListener, ILogger<ServerThread> logger, IConfigurationRoot configuration, int ticksPerSecond = 60) : EngineThread(logger, configuration, ticksPerSecond)
{
    private readonly ServerNetEventListener networking = serverNetEventListener;

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

    protected override void OnThreadTick()
    {
        this.networking.PollEvents();

        base.OnThreadTick();
    }
}