using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;
using Reoria.Engine.Application.Services.Interfaces;
using System.Reflection;
using AppConfigurationSource = Reoria.Engine.Application.Configuration.Interfaces.IAppConfigurationBuilder.AppConfigurationSource;

namespace Reoria.Engine.Application.Configuration;

/// <summary>
/// Defines functions and properties for an application configuration builder.
/// </summary>
public class AppConfigurationBuilder : IAppConfigurationBuilder
{
    /// <summary>
    /// Gets the lock for the configuration builder.
    /// </summary>
    protected virtual Lock Lock { get; init; } = new();
    /// <summary>
    /// Gets the logger for the configuration builder.
    /// </summary>
    protected virtual ILogger<IAppConfigurationBuilder>? Logger { get; set; } = null;
    /// <summary>
    /// Gets the file provider for the configuration builder.
    /// </summary>
    /// <remarks>The file provider set in <see cref="IFileProviderService.FileProvider"/> needs to be set before the class is construted.</remarks>
    protected virtual IFileProvider FileProvider { get; init; } = IFileProviderService.FileProvider;
    /// <summary>
    /// Gets the configuration builder used to build the configuration.
    /// </summary>
    protected virtual IConfigurationBuilder Builder { get; init; } = new ConfigurationBuilder();
    /// <summary>
    /// Gets a collection of configuration sources for the configuration builder.
    /// </summary>
    protected virtual List<AppConfigurationSource> ConfigurationSources { get; init; } = [];
    /// <summary>
    /// Gets the command line arguments for the configuration builder.
    /// </summary>
    protected virtual string[] CommandLineArgs { get; set; } = [];
    /// <inheritdoc />
    public virtual bool UseEnvironmentVariables { get; set; } = true;
    /// <inheritdoc />
    public virtual bool UseUserSecrets { get; set; } = true;

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder AttachLogger(ILogger<IAppConfigurationBuilder> logger)
    {
        // Attach the logger to the configuration builder.
        this.Logger = logger;

        // Check to see if debug logging is enabled.
        if(this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
        {
            // Log a debug message that the logger was attached to the configuration builder.
            this.Logger?.LogDebug("Attached logger to configuration builder.");
        }

        // Return the instance of the configuration builder.
        return this;
    }

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder AddConfigurationSource(AppConfigurationSource source)
    {
        // Lock the configuration builder for thread safety.
        lock(this.Lock)
        {
            // Throw an exception if the source is null.
            ArgumentNullException.ThrowIfNull(source);

            // Check if the source is already added.
            if (this.ConfigurationSources.Contains(source))
            {
                // Return the configuration builder if the source is already added.
                return this;
            }

            // Check if the source is already added.
            if (this.ConfigurationSources.Where(x => x.Path == source.Path).Any())
            {
                // Return the configuration builder if the source is already added.
                return this;
            }

            // Check if the source file exists.
            if(!this.FileProvider.GetFileInfo(source.Path).Exists)
            {
                // Return the configuration builder if the source file does not exist.
                return this;
            }

            // Add the source to the configuration builder.
            this.ConfigurationSources.Add(source);

            // Check to see if debug logging is enabled.
            if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                // Log a debug message that the source was added to the configuration builder.
                this.Logger?.LogDebug("Added configuration source to configuration builder. Path: {Path}, Optional: {Optional}, ReloadOnChange: {ReloadOnChange}.", source.Path, source.Optional, source.ReloadOnChange);
            }

            // Return the configuration builder.
            return this;
        }
    }

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder AddConfigurationSource(string path, bool optional = true, bool reloadOnChange = true)
    {
        // Throw an exception if the path is null or white space.
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Add the source to the configuration builder.
        return this.AddConfigurationSource(new(path, optional, reloadOnChange));
    }

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder AddCommandLine(string[] args)
    {
        // Lock the configuration builder for thread safety.
        lock (this.Lock)
        {
            // Add the command line arguments to the configuration builder.
            this.CommandLineArgs = [.. this.CommandLineArgs, .. args];

            // Return the configuration builder.
            return this;
        }
    }

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder RemoveConfigurationSource(AppConfigurationSource source)
        => this.RemoveConfigurationSource(source.Path);

    /// <inheritdoc />
    public virtual IAppConfigurationBuilder RemoveConfigurationSource(string path)
    {
        // Lock the configuration builder for thread safety.
        lock (this.Lock)
        {
            // Check if the source is already added.
            if (!this.ConfigurationSources.Where(x => x.Path == path).Any())
            {
                // Return the configuration builder if the source is not already added.
                return this;
            }

            // Remove the source from the configuration builder.
            _ = this.ConfigurationSources.RemoveAll(x => x.Path == path);

            // Check to see if debug logging is enabled.
            if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                // Log a debug message that the source was added to the configuration builder.
                this.Logger?.LogDebug("Removed configuration source from configuration builder. Path: {Path}.", path);
            }
        }

        // Return the configuration builder.
        return this;
    }

    /// <inheritdoc />
    public virtual IConfigurationRoot Build()
    {
        // Lock the configuration builder for thread safety.
        lock (this.Lock)
        {
            // Iterate over the configuration sources and add them to the configuration builder.
            foreach (AppConfigurationSource source in this.ConfigurationSources)
            {
                // Add the source to the configuration builder using the file provider.
                _ = this.Builder.AddJsonFile(this.FileProvider, source.Path, source.Optional, source.ReloadOnChange);
            }

            // Check if the environment variables should be used.
            if (this.UseEnvironmentVariables)
            {
                // Add the environment variables to the configuration builder.
                _ = this.Builder.AddEnvironmentVariables();

                // Check to see if debug logging is enabled.
                if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
                {
                    // Log a debug message that environment variables were added to the configuration builder.
                    this.Logger?.LogDebug("Added environment variables to configuration builder.");
                }
            }

            // Check if the user secrets should be used.
            if (this.UseUserSecrets)
            {
                // Add the user secrets to the configuration builder.
                _ = this.Builder.AddUserSecrets(Assembly.GetExecutingAssembly());

                // Check to see if debug logging is enabled.
                if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
                {
                    // Log a debug message that user secrets were added to the configuration builder.
                    this.Logger?.LogDebug("Added user secrets to configuration builder.");
                }
            }

            // Check if any command line arguments were provided.
            if (this.CommandLineArgs.Length > 0)
            {
                // Add the command line arguments to the configuration builder.
                _ = this.Builder.AddCommandLine(this.CommandLineArgs);

                // Check to see if debug logging is enabled.
                if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
                {
                    // Log a debug message that the source was added to the configuration builder.
                    this.Logger?.LogDebug("Added command line arguments to configuration builder. Arguments added: {args}", this.CommandLineArgs.ToString());
                }
            }

            // Check to see if debug logging is enabled.
            if (this.Logger?.IsEnabled(LogLevel.Debug) ?? false)
            {
                // Log a debug message that the source was added to the configuration builder.
                this.Logger?.LogDebug("Compiling sources and building game engine configuration.");
            }

            // Build the configuration and return it.
            return this.Builder.Build();
        }
    }
}