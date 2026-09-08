using Microsoft.Extensions.Configuration;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Phases;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for adding configuration functionality to an application.
/// </summary>
/// <remarks>
/// These extensions provide configuration building capabilities for applications,
/// allowing phase participants to participate in the configuration process and add custom
/// configuration sources or transformers.
/// </remarks>
public static class ApplicationConfigurationExtensions
{
    /// <summary>
    /// Gets the configuration for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>An instance of <see cref="IConfiguration"/>.</returns>
    /// <exception cref="InvalidOperationException">Raised when the configuration cannot be built.</exception>
    /// <remarks>
    /// This method builds the application configuration by:
    /// 1. Creating a new configuration builder
    /// 2. Adding default configuration sources (appsettings.json files)
    /// 3. Invoking bootstrap configuration phase participants
    /// 4. Adding command-line arguments
    /// 5. Building the final configuration
    /// </remarks>
    public static IConfiguration GetConfiguration(this IApplication application, string[] args)
    {
        // Create a new configuration builder.
        AppConfigurationBuilder builder = new();

        // Add the default configuration sources.
        _ = builder.AddConfigurationSource("appsettings.json", false, true);
        _ = builder.AddConfigurationSource("appsettings.logging.json", true, true);
        _ = builder.AddConfigurationSource("appsettings.serilog.json", true, true);

        // Invoke bootstrap configuration phase participants to add custom configuration sources.
        application.PhaseService.ExecutePhase<IBootstrapConfiguration>(phase => phase.OnBuildConfiguration(builder));

        // Add command-line arguments to override configuration values.
        _ = builder.AddCommandLine(args);

        // Build the configuration and return it.
        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }
}