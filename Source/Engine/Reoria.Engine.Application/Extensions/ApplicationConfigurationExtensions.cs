using Microsoft.Extensions.Configuration;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for adding configuration functionality to an application.
/// </summary>
public static class ApplicationConfigurationExtensions
{
    /// <summary>
    /// Gets the configuration for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>An instance of <see cref="IConfiguration"/>.</returns>
    /// <exception cref="InvalidOperationException">Raised when the configuration cannot be built.</exception>
    public static IConfiguration GetConfiguration(this IApplication application, string[] args)
    {
        // Create a new configuration builder.
        AppConfigurationBuilder builder = new();

        // Add the default configuration sources.
        _ = builder.AddConfigurationSource("appsettings.json", false, true);
        _ = builder.AddConfigurationSource("appsettings.logging.json", true, true);
        _ = builder.AddConfigurationSource("appsettings.serilog.json", true, true);

        // Iterate over the injectors.
        foreach (IApplicationConfigurationInjector injector in application.Injectors.OfType<IApplicationConfigurationInjector>())
        {
            // Invoke the injector.
            injector.OnGetConfiguration(builder);
        }

        // Add the command-line arguments.
        _ = builder.AddCommandLine(args);

        // Build the configuration and return it.
        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }
}