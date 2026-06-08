using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Serilog;
using Serilog.Extensions.Logging;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for adding logging functionality to an application.
/// </summary>
/// <remarks>
/// These extensions provide logging setup capabilities for applications,
/// allowing injectors to participate in the logging configuration process and add
/// custom logging providers or formatters.
/// </remarks>
public static class ApplicationLoggingExtensions
{
    /// <summary>
    /// Gets the logger factory for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="ILoggerFactory"/>.</returns>
    /// <remarks>
    /// This method creates and configures the logger factory by:
    /// 1. Creating a new logger factory
    /// 2. Closing and flushing any existing Serilog logger
    /// 3. Creating a new Serilog logger based on configuration
    /// 4. Adding the Serilog provider to the logger factory
    /// 5. Invoking application logging injectors for custom configuration
    /// </remarks>
    public static ILoggerFactory GetLoggerFactory(this IApplication application)
    {
        // Create the logger factory.
        LoggerFactory loggerFactory = new();

        // Close and flush any existing Serilog logger to prevent conflicts.
        Log.CloseAndFlush();

        // Create a new Serilog logger based on application configuration.
        Log.Logger = application.Configuration is not null
            ? new LoggerConfiguration()
                .ReadFrom.Configuration(application.Configuration)
                .CreateLogger()
            : new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        // Add the Serilog logger to the logger factory as a provider.
        loggerFactory.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Invoke application logging injectors to add custom logging providers or configuration.
        application.InjectorService.ExecuteInjectors<IApplicationLoggingInjector>(injector => injector.OnCreateLoggerFactory(loggerFactory, application.Configuration!));

        // Return the configured logger factory.
        return loggerFactory;
    }
}