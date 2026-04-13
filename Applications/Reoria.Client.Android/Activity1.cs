using Android.Content.PM;
using Android.Views;
using Microsoft.Xna.Framework;
using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Services.Interfaces;
using Reoria.Engine.Core.Configuration.Providers;

namespace Reoria.Client.Android;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.FullUser,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
)]
public class Activity1 : AndroidGameActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        IFileProviderService.SetFileProvider(new AndroidAssetFileProvider(this.Assets!));

        using ClientApplication application = new AppBootStrapper(Platform.Android, []).CreateApplication<ClientApplication>();
        View? view = application.Services.GetService(typeof(View)) as View;

        this.SetContentView(view ?? throw new InvalidOperationException("Unable to resolve view."));
        application.Run();
    }
}
