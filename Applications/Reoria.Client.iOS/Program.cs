using Reoria.Engine.Application;
using Reoria.Engine.Application.Threads;

namespace Reoria.Client.iOS;

[Register("AppDelegate")]
internal class Program : UIApplicationDelegate
{
    private static string[] Args = [];

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
        Args = args;
        UIApplication.Main(args, null, typeof(Program));
    }

    public override void FinishedLaunching(UIApplication app)
    {
        IGameThread engine = new AppBuilder(Args).Build();
        engine.Run();
    }
}
