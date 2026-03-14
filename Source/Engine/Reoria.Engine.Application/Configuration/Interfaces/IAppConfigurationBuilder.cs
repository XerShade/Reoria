using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Reoria.Engine.Application.Configuration.Interfaces;

/// <summary>
/// Defines an abstraction contract for an application configuration builder.
/// </summary>
public partial interface IAppConfigurationBuilder
{
    /// <summary>
    /// Gets or sets a value indicating whether to use user secrets.
    /// </summary>
    bool UseUserSecrets { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether to use environment variables.
    /// </summary>
    bool UseEnvironmentVariables { get; set; }

    /// <summary>
    /// Adds command line arguments to the configuration builder.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder AddCommandLine(string[] args);
    /// <summary>
    /// Adds a configuration source to the configuration builder.
    /// </summary>
    /// <param name="source">The configuration source to add.</param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder AddConfigurationSource(AppConfigurationSource source);
    /// <summary>
    /// Adds a new configuration source to the configuration builder.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <param name="optional">Indicates whether the configuration file is optional.</param>
    /// <param name="reloadOnChange">Indicates whether to reload the configuration file when it changes.</param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder AddConfigurationSource(string path, bool optional = true, bool reloadOnChange = true);
    /// <summary>
    /// Attaches a logger to the configuration builder.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to attach.</param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder AttachLogger(ILogger<IAppConfigurationBuilder> logger);
    /// <summary>
    /// Builds an <see cref="IConfigurationRoot"/> instance from the configuration builder.
    /// </summary>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IConfigurationRoot Build();
    /// <summary>
    /// Removes the configuration source from the configuration builder.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder RemoveConfigurationSource(AppConfigurationSource source);
    /// <summary>
    /// Removes the configuration source with the specified path from the configuration builder.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <returns>The <see cref="IAppConfigurationBuilder"/> instance for method chaining.</returns>
    IAppConfigurationBuilder RemoveConfigurationSource(string path);

    /// <summary>
    /// Defines a record for an application configuration source.
    /// </summary>
    /// <param name="Path">The path to the configuration file.</param>
    /// <param name="Optional">Indicates whether the configuration file is optional.</param>
    /// <param name="ReloadOnChange">Indicates whether to reload the configuration file when it changes.</param>
    public record AppConfigurationSource(string Path, bool Optional = true, bool ReloadOnChange = true)
    {
        /// <summary>
        /// Gets or sets the path to the configuration file.
        /// </summary>
        public string Path { get; init; } = Path ?? string.Empty;
        /// <summary>
        /// Gets or sets a value indicating whether the configuration file is optional.
        /// </summary>
        public bool Optional { get; init; } = Optional;
        /// <summary>
        /// Gets or sets a value indicating whether to reload the configuration file when it changes.
        /// </summary>
        public bool ReloadOnChange { get; init; } = ReloadOnChange;
    }
}
