using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Serilog;
using Serilog.Extensions.Logging;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for adding logging functionality to an application.
/// </summary>
public static class ApplicationLoggingExtensions
{
    /// <summary>
    /// Gets the logger factory for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="ILoggerFactory"/>.</returns>
    public static ILoggerFactory GetLoggerFactory(this IApplication application)
    {
        // Create the logger factory.
        LoggerFactory loggerFactory = new();

        // Close and flush the serilog logger.
        Log.CloseAndFlush();

        // Create the serilog logger.
        Log.Logger = application.Configuration is not null
            ? new LoggerConfiguration()
                .ReadFrom.Configuration(application.Configuration)
                .CreateLogger()
            : new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        // Add the serilog logger to the logger factory.
        loggerFactory.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Iterate over the injectors.
        foreach (IApplicationLoggingInjector injector in application.Injectors.OfType<IApplicationLoggingInjector>())
        {
            // Invoke the injector.
            injector.OnGetLoggerFactory(loggerFactory, application.Configuration!);
        }

        // Return the logger factory.
        return loggerFactory;
    }
}