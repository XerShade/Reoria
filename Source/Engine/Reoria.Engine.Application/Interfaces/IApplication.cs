using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Services.Interfaces;

namespace Reoria.Engine.Application.Interfaces;

/// <summary>
/// Defines an abstraction contract for a class that represents an application.
/// </summary>
/// <remarks>
/// This interface provides the foundation for all Reoria applications, including client and server variants.
/// It defines the core properties and methods required for application lifecycle management,
/// dependency injection setup, configuration access, and platform abstraction.
/// All applications in the Reoria ecosystem must implement this interface to ensure
/// consistent behavior across different platforms (Windows, Desktop, Android, iOS, Server).
/// </remarks>
public interface IApplication : IDisposable
{
    /// <summary>
    /// Gets the platform that the application is running on.
    /// </summary>
    /// <remarks>
    /// This property identifies the target platform (Windows, Desktop, Android, iOS, Server)
    /// and allows platform-specific code paths to be executed. Platform detection is crucial
    /// for handling differences in input methods, file system access, and rendering capabilities.
    /// </remarks>
    Platform Platform { get; }

    /// <summary>
    /// Gets an instance of <see cref="ILogger"/> that can be used to log messages.
    /// </summary>
    /// <remarks>
    /// Provides application-specific logging capabilities. The logger is pre-configured with the
    /// application's context and can be used for debugging, error reporting, and audit trails.
    /// </remarks>
    ILogger<IApplication> Logger { get; }

    /// <summary>
    /// Gets an instance of <see cref="IInjectorService"/> that can be used to inject functionality.
    /// </summary>
    /// <remarks>
    /// Provides a mechanism for registering and resolving services and components in the application,
    /// allowing for loose coupling and dependency injection and allowing code injection to add
    /// functionality to the application.
    /// </remarks>
    IInjectorService InjectorService { get; }

    /// <summary>
    /// Gets an instance of <see cref="IConfiguration"/> that can be used to access configuration settings.
    /// </summary>
    /// <remarks>
    /// Provides access to application configuration from various sources (appsettings.json, environment variables,
    /// command line arguments). Configuration is hierarchical and supports different environments
    /// (Development, Staging, Production).
    /// </remarks>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Gets an instance of <see cref="ILoggerFactory"/> that can be used to create loggers.
    /// </summary>
    /// <remarks>
    /// Factory for creating typed loggers for different components. This allows components
    /// to create their own loggers with proper category names and configuration.
    /// </remarks>
    ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Gets an instance of <see cref="ContainerBuilder"/> that can be used to register services.
    /// </summary>
    /// <remarks>
    /// Autofac container builder for dependency injection registration. This allows components
    /// to register services and their lifetimes (singleton, scoped, transient) before
    /// the container is built and the application starts.
    /// </remarks>
    ContainerBuilder ContainerBuilder { get; }

    /// <summary>
    /// Gets an instance of <see cref="IServiceProvider"/> that can be used to resolve services.
    /// </summary>
    /// <remarks>
    /// Built dependency injection container that provides service resolution. This is the primary
    /// mechanism for components to obtain dependencies and maintain loose coupling.
    /// </remarks>
    IServiceProvider Provider { get; }

    /// <summary>
    /// Runs the application.
    /// </summary>
    /// <remarks>
    /// Starts the main application loop and initializes all core systems.
    /// For client applications, this typically initializes graphics and starts the game loop.
    /// For server applications, this starts network listeners and begins processing client connections.
    /// This method should block until the application is ready to exit.
    /// </remarks>
    void Run();

    /// <summary>
    /// Requests the application to exit gracefully.
    /// </summary>
    /// <remarks>
    /// Signals the application to terminate at the next appropriate opportunity.
    /// This allows for graceful shutdown, cleanup of resources, and proper disposal
    /// of components. The exact timing depends on the application type and current state.
    /// </remarks>
    void Exit();
}