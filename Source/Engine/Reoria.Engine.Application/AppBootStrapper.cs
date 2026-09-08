using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Application.Services;
using Reoria.Engine.Application.Services.Interfaces;
using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines methods and properties for an application bootstrapper used to create a simple dependency injection container.
/// </summary>
/// <param name="platform">The platform that the application is running on.</param>
/// <param name="args">Gets the command-line arguments passed to the application.</param>
/// <remarks>This step of the application life cycle should only load the bare minimum of dependencies and configuration required to bootstrap the application.</remarks>
public partial class AppBootStrapper(Platform platform, string[] args)
{
    /// <summary>
    /// Gets the platform that the application is running on.
    /// </summary>
    protected Platform Platform { get; set; } = platform.HasSingleFlag() ? platform : throw new InvalidOperationException("Only one Platform value can be selected.");

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
    /// Creates and returns the application instance.
    /// </summary>
    /// <typeparam name="TApplication">The application type to run.</typeparam>
    /// <returns>The application instance.</returns>
    public virtual TApplication CreateApplication<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication>() 
        where TApplication : class, IApplication
    {
        // Start a new stopwatch to measure the bootstrapping time.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Get the configuration instance.
        IConfiguration configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        ILoggerFactory loggerFactory = this.GetLoggerFactory(configuration);
        ILogger<AppBootStrapper> logger = this.GetLogger(loggerFactory);

        // Get the service collection and service provider instances.
        ContainerBuilder services = this.GetServices<TApplication>(args, configuration, loggerFactory);
        IServiceProvider provider = this.GetServiceProvider(services);

        // Stop the stopwatch to measure the bootstrapping time.
        stopwatch.Stop();

        // Check if logging is enabled.
        if (logger.IsEnabled(LogLevel.Information))
        {
            // Report the bootstrapping completion.
            logger.LogInformation("{LogoArt}", Environment.NewLine + (this.LogoArt ?? "----- Reoria Game Engine -----"));
            logger.LogInformation("Bootstrapping completed, it took {time} ms.", stopwatch.ElapsedMilliseconds);
        }

        // Return the application instance from the service provider.
        return provider.GetRequiredService<IApplication>() as TApplication ?? throw new InvalidOperationException("Unable to get the application instance.");
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

        // Add the command-line arguments.
        _ = builder.AddCommandLine(args);

        // Execute bootstrap configuration phase participants to add additional configuration sources.
        IPhaseService phaseService = new PhaseService().AddAssemblies(AppDomain.CurrentDomain.GetAssemblies()).SetPlatform(this.Platform);
        phaseService.ExecutePhase<IBootstrapConfiguration>(phase => phase.OnBuildConfiguration(builder));

        // Build the configuration and return it.
        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }

    /// <summary>
    /// Creates and builds the <see cref="ILoggerFactory"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <returns>The built <see cref="ILoggerFactory"/> instance.</returns>
    protected virtual ILoggerFactory GetLoggerFactory(IConfiguration configuration)
    {
        // Create a new logger factory.
        LoggerFactory loggerFactory = new();

        // Close and flush the Serilog logger.
        Log.CloseAndFlush();

        // Create a new Serilog logger.
        Log.Logger = configuration is not null
            ? new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger()
            : new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        // Add the Serilog logger to the logger factory.
        loggerFactory.AddProvider(new SerilogLoggerProvider(Log.Logger));

        // Return the logger factory.
        return loggerFactory;
    }

    /// <summary>
    /// Creates and builds the <see cref="Microsoft.Extensions.Logging.ILogger"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance.</param>
    /// <returns>The built <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.</returns>
    protected ILogger<AppBootStrapper> GetLogger(ILoggerFactory loggerFactory)
        // Use the logger factory to create a logger for the bootstrapper.
        => loggerFactory.CreateLogger<AppBootStrapper>();

    /// <summary>
    /// Creates and builds the <see cref="ContainerBuilder"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The built <see cref="ContainerBuilder"/> instance.</returns>
    protected virtual ContainerBuilder GetServices<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication>
        (string[] args, IConfiguration configuration, ILoggerFactory loggerFactory) where TApplication : class, IApplication
    {
        // Create a new autofac container builder.
        ContainerBuilder services = new();

        // Register the configuration and logger factories as singletons.
        _ = services.RegisterInstance(configuration)
            .As<IConfiguration>()
            .SingleInstance();
        _ = services.RegisterInstance(loggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();
        _ = services.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        // Register the command-line arguments and app boot context as singletons.
        _ = services.RegisterInstance(new AppBootContext(this.Platform, args))
            .Keyed<AppBootContext>("AppContext")
            .As<AppBootContext>()
            .SingleInstance();

        // Register the application as a singleton.
        _ = services.RegisterType<TApplication>()
            .As<IApplication>()
            .SingleInstance();

        // Execute bootstrap services phase participants to register additional services.
        IPhaseService phaseService = new PhaseService().AddAssemblies(AppDomain.CurrentDomain.GetAssemblies()).SetPlatform(this.Platform);
        phaseService.ExecutePhase<IBootstrapServices>(phase => phase.OnRegisterServices(services));

        // Return the container builder.
        return services;
    }

    /// <summary>
    /// Creates and builds the <see cref="IServiceProvider"/> instance for the bootstrapper.
    /// </summary>
    /// <param name="services">The <see cref="ContainerBuilder"/> instance.</param>
    /// <returns>The built <see cref="IServiceProvider"/> instance.</returns>
    protected virtual IServiceProvider GetServiceProvider(ContainerBuilder services)
    {
        // Build the autofac container.
        IContainer container = services.Build();

        // Create a new autofac service provider.
        AutofacServiceProvider provider = new(container);

        // Return the autofac service provider.
        return provider;
    }
}