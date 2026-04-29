using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Client.iOS;

/// <summary>
/// iOS application delegate and entry point for Reoria iOS Client.
/// </summary>
/// <remarks>
/// This class handles the iOS application lifecycle and initialization.
/// iOS applications use a different startup pattern than desktop applications,
/// requiring integration with UIKit's application delegate pattern.
/// The iOS platform provides touch input, accelerometer, and other mobile-specific features.
/// </remarks>
[Register("AppDelegate")]
internal class Program : UIApplicationDelegate
{
    /// <summary>
    /// The main entry point for the iOS application.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application (typically empty on iOS).</param>
    /// <remarks>
    /// This method is called when the application starts and initializes the UIKit application.
    /// It sets up the main application delegate and starts the iOS event loop.
    /// </remarks>
    static void Main(string[] args) => UIApplication.Main(args, null, typeof(Program));

    /// <summary>
    /// Called after the iOS application has finished launching.
    /// </summary>
    /// <param name="app">The UIApplication instance that launched this delegate.</param>
    /// <remarks>
    /// This method initializes the Reoria client application for iOS platform.
    /// It sets up dependency injection bootstrapper, creates the client application,
    /// and starts the main game loop. iOS applications must handle this differently
    /// than desktop applications due to the mobile platform's lifecycle management.
    /// </remarks>
    public override void FinishedLaunching(UIApplication app)
    {
        try
        {
            // Initialize application bootstrapper for iOS platform
            // This sets up dependency injection, logging, configuration, and services
            // Note: iOS apps typically don't use command line arguments
            AppBootStrapper bootstrapper = new(Platform.iOS, []);

            // Create and configure the client application instance
            using ClientApplication application = bootstrapper.CreateApplication<ClientApplication>();

            // Start the main application loop
            // This will initialize graphics, networking, and begin the game loop
            // On iOS, this integrates with the UIKit event loop
            application.Run();
        }
        catch (Exception ex)
        {
            // Log the error for debugging purposes
            // In production, this should use iOS-specific logging or crash reporting
            Console.Error.WriteLine($"Fatal error starting iOS client: {ex.Message}");

            // On iOS, we might want to show an alert dialog to the user
            // This would require additional UIKit integration
        }
    }
}