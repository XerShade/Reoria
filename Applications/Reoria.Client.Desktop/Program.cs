using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;

/// <summary>
/// Entry point for the Reoria Desktop Client application.
/// </summary>
/// <remarks>
/// This program initializes and runs the desktop version of the Reoria client.
/// It uses the dependency injection bootstrapper to configure and create the client application,
/// then starts the main game loop. The desktop platform provides full keyboard and mouse
/// input support along with windowed display capabilities.
/// </remarks>
public static class Program
{
    /// <summary>
    /// The main entry point for the desktop client application.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>Exit code indicating application termination status.</returns>
    public static int Main(string[] args)
    {
        try
        {
            // Initialize application bootstrapper for desktop platform
            // This sets up dependency injection, logging, configuration, and services
            AppBootStrapper bootstrapper = new(Platform.Desktop, args);
            
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
            Console.Error.WriteLine($"Fatal error starting desktop client: {ex.Message}");
            return 1; // Error exit code
        }
    }
}
