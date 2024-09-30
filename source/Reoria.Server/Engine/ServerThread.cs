using Reoria.Engine;

namespace Reoria.Server.Engine;

public class ServerThread : EngineThread
{
    private readonly ServerNetEventListener networking = new();

    protected override void OnThreadStart()
    {
        this.networking.Start(5555);
        Console.WriteLine($"Started server on {this.networking.GetLocalPort()}");

        base.OnThreadStart();
    }

    protected override void OnThreadStop()
    {
        this.networking.Stop();
        Console.WriteLine($"Stopped listening on {this.networking.GetLocalPort()}");

        base.OnThreadStop();
    }

    protected override void OnThreadTick()
    {
        this.networking.PollEvents();

        base.OnThreadTick();
    }
}
