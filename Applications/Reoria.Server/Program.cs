using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Interfaces;
using Reoria.Server.Core.Application;

/// <summary>
/// Entry point for the Reoria Server application.
/// </summary>
/// <remarks>
/// This program initializes and runs the Reoria multiplayer game server.
/// It uses dependency injection bootstrapper to configure and create the server application,
/// then starts the main server loop. The server handles client connections, game state
/// synchronization, and server-authoritative game logic processing.
/// </remarks>
public static class Program
{
    /// <summary>
    /// The main entry point for the server application.
    /// </summary>
    /// <param name="args">Command line arguments passed to the server application.</param>
    /// <returns>Exit code indicating server termination status.</returns>
    public static int Main(string[] args)
    {
        try
        {
            // Initialize application bootstrapper for server platform
            // This sets up dependency injection, logging, configuration, and networking services
            AppBootStrapper bootstrapper = new(Platform.Server, args);
            
            // Create and configure the server application instance
            using IApplication application = bootstrapper.CreateApplication<ServerApplication>();
            
            // Start the main server application loop
            // This will initialize networking, start listening for client connections,
            // and begin processing game logic updates
            application.Run();
            
            return 0; // Successful exit
        }
        catch (Exception ex)
        {
            // Log the error and return failure code
            // In production, this should use proper logging infrastructure
            Console.Error.WriteLine($"Fatal error starting server: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1; // Error exit code
        }
    }
}
