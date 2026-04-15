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
    private ClientApplication? application;

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        IFileProviderService.SetFileProvider(new AndroidAssetFileProvider(this.Assets!));

        this.application = new AppBootStrapper(Platform.Android, []).CreateApplication<ClientApplication>();
        View? view = this.application.Services.GetService(typeof(View)) as View;

        this.SetContentView(view ?? throw new InvalidOperationException("Unable to resolve view."));
        this.application.Run();
    }

    protected override void OnResume()
    {
        try
        {
            base.OnResume();
        }
        catch (NullReferenceException ex)
        {
            // Handle the case where GraphicsDeviceManager is not yet initialized
            // This is a known issue in MonoGame Android when ForceSetFullScreen is called too early
            System.Diagnostics.Debug.WriteLine($"NullReferenceException in OnResume: {ex.Message}");
        }
    }

    protected override void OnDestroy()
    {
        try
        {
            this.application?.Dispose();
            this.application = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
        }
        finally
        {
            base.OnDestroy();
        }
    }
}
