using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Client.iOS;

[Register("AppDelegate")]
internal class Program : UIApplicationDelegate
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(Program));
    }

    public override void FinishedLaunching(UIApplication app)
    {
        using ClientApplication application = new AppBootStrapper(Platform.iOS, []).CreateApplication<ClientApplication>();
        application.Run();
    }
}
