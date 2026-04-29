using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;

namespace Reoria.Client.Windows;

/// <summary>
/// Entry point for the Reoria Windows Client application.
/// </summary>
/// <remarks>
/// This program initializes and runs the Windows version of the Reoria client.
/// It uses dependency injection bootstrapper to configure and create the client application,
/// then starts the main game loop. The Windows platform provides full keyboard and mouse
/// input support along with Windows-specific display and window management capabilities.
/// </remarks>
public static class Program
{
    /// <summary>
    /// The main entry point for the Windows client application.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>Exit code indicating application termination status.</returns>
    public static int Main(string[] args)
    {
        try
        {
            // Initialize application bootstrapper for Windows platform
            // This sets up dependency injection, logging, configuration, and services
            AppBootStrapper bootstrapper = new(Platform.Windows, args);

            // Create and configure the client application instance
            using ClientApplication application = bootstrapper.CreateApplication<ClientApplication>();

            // Start the main application loop
            // This will initialize graphics, networking, and begin the game loop
            application.Run();

            return 0; // Successful exit
        }
        catch (Exception ex)
        {
            // In a production environment, you might want to log this error
            // and display a user-friendly error message
            Console.Error.WriteLine($"Fatal error starting Windows client: {ex.Message}");
            return 1; // Error exit code
        }
    }
}