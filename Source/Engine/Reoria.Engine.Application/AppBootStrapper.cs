using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Services;
using Reoria.Engine.Application.Services.Interfaces;
using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines methods and properties for an application bootstrapper used to create a simple dependency injection container.
/// </summary>
/// <remarks>This step of the application life cycle should only load the bare minimum of dependencies and configuration required to bootstrap the application.</remarks>
public partial class AppBootStrapper
{
    /// <summary>
    /// Gets the platform that the application is running on.
    /// </summary>
    protected Platform Platform { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="IInjectorService"/> that can be used to inject bootstrap services.
    /// </summary>
    protected IInjectorService InjectorService { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="IConfiguration"/> that can be used to bootstrap the application.
    /// </summary>
    protected IConfiguration Configuration { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="ILoggerFactory"/> that can be used to create loggers.
    /// </summary>
    protected ILoggerFactory LoggerFactory { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="ILogger{TCategoryName}"/> that can be used to log messages.
    /// </summary>
    protected ILogger<AppBootStrapper> Logger { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="ContainerBuilder"/> that can be used to register bootstrap services.
    /// </summary>
    protected ContainerBuilder Services { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="IServiceProvider"/> that can be used to resolve bootstrap services.
    /// </summary>
    protected IServiceProvider Provider { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppBootStrapper"/> class.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public AppBootStrapper(Platform platform, string[] args)
    {
        // Start a new stopwatch to measure the bootstrapping time.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Check to see if only one Platform value is selected, and if it is, assign it to the Platform property.
        this.Platform = platform.HasSingleFlag() ? platform : throw new InvalidOperationException("Only one Platform value can be selected.");

        // Create the injector service.
        this.InjectorService = new InjectorService()
            .AddAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .SetPlatform(platform);

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.GetLogger();

        // Get the service collection and service provider instances.
        this.Services = this.GetServices(args);
        this.Provider = this.GetServiceProvider();

        // Stop the stopwatch to measure the bootstrapping time.
        stopwatch.Stop();

        // Report the bootstrapping completion.
        this.ReportBootStrappingCompletion(stopwatch);
    }

    /// <summary>
    /// Creates and builds the <see cref="IConfiguration"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The built <see cref="IConfiguration"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configuration fails to build.</exception>
    protected virtual IConfiguration GetConfiguration(string[] args)
    {
        // Create a new configuration builder.
        AppConfigurationBuilder builder = new();

        // Add the default configuration sources.
        _ = builder.AddConfigurationSource("appsettings.json", false, true);
        _ = builder.AddConfigurationSource("appsettings.logging.json", true, true);
        _ = builder.AddConfigurationSource("appsettings.serilog.json", true, true);

        // Execute configuration injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapConfigurationInjector>(injector => injector.OnBuildConfiguration(builder));

        // Add the command-line arguments.
        _ = builder.AddCommandLine(args);

        // Build the configuration and return it.
        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }

    /// <summary>
    /// Creates and builds the <see cref="ILoggerFactory"/> instance for the bootstrapper.
    /// </summary>
    /// <returns>The built <see cref="ILoggerFactory"/> instance.</returns>
    protected virtual ILoggerFactory GetLoggerFactory()
    {
        // Create a new logger factory.
        LoggerFactory loggerFactory = new();

        // Close and flush the Serilog logger.
        Log.CloseAndFlush();

        // Create a new Serilog logger.
        Log.Logger = this.Configuration is not null
            ? new LoggerConfiguration()
                .ReadFrom.Configuration(this.Configuration)
                .CreateLogger()
            : new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        // Add the Serilog logger to the logger factory.
        loggerFactory.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Execute logging injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapLoggingInjector>(injector => injector.OnCreateLoggerFactory(loggerFactory, this.Configuration!));

        // Return the logger factory.
        return loggerFactory;
    }

    /// <summary>
    /// Creates and builds the <see cref="Microsoft.Extensions.Logging.ILogger"/> instance for the bootstrapper.
    /// </summary>
    /// <returns>The built <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.</returns>
    protected ILogger<AppBootStrapper> GetLogger()
        // Use the logger factory to create a logger for the bootstrapper.
        => this.LoggerFactory.CreateLogger<AppBootStrapper>();

    /// <summary>
    /// Creates and builds the <see cref="ContainerBuilder"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The built <see cref="ContainerBuilder"/> instance.</returns>
    protected virtual ContainerBuilder GetServices(string[] args)
    {
        // Create a new autofac container builder.
        ContainerBuilder services = new();

        // Register the configuration and logger factories as singletons.
        _ = services.RegisterInstance(this.Configuration)
            .As<IConfiguration>()
            .SingleInstance();
        _ = services.RegisterInstance(this.LoggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();
        _ = services.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        // Register the command-line arguments and app boot context as singletons.
        _ = services.RegisterInstance(args)
            .Keyed<string[]>("CommandLineArgs")
            .As<string[]>()
            .SingleInstance();
        _ = services.RegisterInstance(new AppBootContext(this.Platform))
            .Keyed<AppBootContext>("AppContext")
            .As<AppBootContext>()
            .SingleInstance();

        // Register the injector service as a singleton for use by other parts of the application.
        _ = services.RegisterInstance(this.InjectorService)
            .As<IInjectorService>()
            .As<InjectorService>()
            .SingleInstance();

        // Execute services injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapServicesInjector>(injector => injector.OnBuildServices(services));

        // Execute application injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapApplicationInjector>(injector => injector.OnBuildServices(services));

        // Return the container builder.
        return services;
    }

    /// <summary>
    /// Creates and builds the <see cref="IServiceProvider"/> instance for the bootstrapper.
    /// </summary>
    /// <returns>The built <see cref="IServiceProvider"/> instance.</returns>
    protected virtual IServiceProvider GetServiceProvider()
    {
        // Build the autofac container.
        IContainer container = this.Services.Build();

        // Create a new autofac service provider.
        AutofacServiceProvider provider = new(container);

        // Execute services injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapServicesInjector>(injector => injector.OnConfigureServices(provider));

        // Execute application injectors.
        this.InjectorService.ExecuteInjectors<IBootStrapApplicationInjector>(injector => injector.OnConfigureServices(provider));

        // Return the autofac service provider.
        return provider;
    }

    /// <summary>
    /// Gets an instance of <see cref="Microsoft.Extensions.Logging.ILogger"/> for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to get the logger for.</typeparam>
    /// <returns>The <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.</returns>
    public virtual ILogger<T> GetLogger<T>()
        // Use the service provider to get a logger for the specified type.
        => this.Provider.GetRequiredService<ILogger<T>>();

    /// <summary>
    /// Gets the ASCII logo art for the application.
    /// </summary>
    protected string LogoArt { get; init; } =
        "" +
        "  ____                 _          ____                        _____             _            \r\n" +
        " |  _ \\ ___  ___  _ __(_) __ _   / ___| __ _ _ __ ___   ___  | ____|_ __   __ _(_)_ __   ___ \r\n" +
        " | |_) / _ \\/ _ \\| '__| |/ _` | | |  _ / _` | '_ ` _ \\ / _ \\ |  _| | '_ \\ / _` | | '_ \\ / _ \\\r\n" +
        " |  _ <  __/ (_) | |  | | (_| | | |_| | (_| | | | | | |  __/ | |___| | | | (_| | | | | |  __/\r\n" +
        " |_| \\_\\___|\\___/|_|  |_|\\__,_|  \\____|\\__,_|_| |_| |_|\\___| |_____|_| |_|\\__, |_|_| |_|\\___|\r\n" +
        "                                                                          |___/              " +
        "";

    /// <summary>
    /// Reports the bootstrapping completion and statistics to the logger.
    /// </summary>
    /// <param name="stopwatch">The stopwatch used to measure the bootstrapping time.</param>
    protected virtual void ReportBootStrappingCompletion(Stopwatch stopwatch)
    {
        // Check if logging is enabled.
        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            // Report the bootstrapping completion.
            this.Logger.LogInformation("{LogoArt}", Environment.NewLine + (this.LogoArt ?? "----- Reoria Game Engine -----"));
            this.Logger.LogInformation("Bootstrapping completed, it took {time} ms.", stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Creates and returns the application instance.
    /// </summary>
    /// <typeparam name="TApplication">The application type to run.</typeparam>
    /// <returns>The application instance.</returns>
    public virtual TApplication CreateApplication<TApplication>() where TApplication : class, IApplication
        => this.Provider.GetRequiredService<TApplication>();
}