using Android.Content.PM;
using Android.Views;
using Microsoft.Xna.Framework;
using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Services.Interfaces;
using Reoria.Engine.Core.Configuration.Providers;

namespace Reoria.Client.Android;

/// <summary>
/// Main activity for Reoria Android Client application.
/// </summary>
/// <remarks>
/// This class serves as the entry point and lifecycle manager for the Android version of Reoria client.
/// Android applications use activities as the fundamental building block of the UI, and this activity
/// integrates MonoGame's AndroidGameActivity with Reoria's client application framework.
/// The Android platform provides touch input, accelerometer, and other mobile-specific features.
/// </remarks>
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
    /// <summary>
    /// The Reoria client application instance.
    /// </summary>
    private ClientApplication? application;

    /// <summary>
    /// Called when the activity is first created.
    /// </summary>
    /// <param name="bundle">Bundle containing the activity's previously saved state, if any.</param>
    /// <remarks>
    /// This method initializes the Android activity and sets up the Reoria client application.
    /// It configures the Android asset file provider, creates the client application through
    /// dependency injection bootstrapper, and starts the main game loop.
    /// </remarks>
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        // Configure Android-specific file provider for asset access
        // This allows the game to load content from Android's assets folder
        IFileProviderService.SetFileProvider(new AndroidAssetFileProvider(this.Assets!));

        // Initialize application bootstrapper for Android platform
        // This sets up dependency injection, logging, configuration, and services
        // Note: Android apps typically don't use command line arguments
        this.application = new AppBootStrapper(Platform.Android, []).CreateApplication<ClientApplication>();
        
        // Get the MonoGame view from the application services
        // This view contains the game rendering surface
        View? view = this.application.Services.GetService(typeof(View)) as View;

        // Set the activity's content view to the game view
        // This makes the game visible and interactive
        this.SetContentView(view ?? throw new InvalidOperationException("Unable to resolve view."));
        
        // Start the main application loop
        // This begins game initialization and starts the update/render cycle
        this.application.Run();
    }

    /// <summary>
    /// Called when the activity is being resumed after being stopped or paused.
    /// </summary>
    /// <remarks>
    /// This method handles the Android activity lifecycle event when the app comes to the foreground.
    /// It includes error handling for a known MonoGame Android issue where GraphicsDeviceManager
    /// may not be initialized when ForceSetFullScreen is called too early in the lifecycle.
    /// </remarks>
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

    /// <summary>
    /// Called when the activity is being destroyed.
    /// </summary>
    /// <remarks>
    /// This method performs cleanup when the activity is being removed from memory.
    /// It properly disposes of the client application and its resources to prevent memory leaks.
    /// Proper cleanup is especially important on mobile devices with limited memory.
    /// </remarks>
    protected override void OnDestroy()
    {
        try
        {
            // Dispose of the client application and its resources
            this.application?.Dispose();
            this.application = null;
        }
        catch (Exception ex)
        {
            // Log any errors during disposal for debugging
            System.Diagnostics.Debug.WriteLine($"Error during disposal: {ex.Message}");
        }
        finally
        {
            // Always call the base implementation
            base.OnDestroy();
        }
    }
}
