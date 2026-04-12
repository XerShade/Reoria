using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Injectors;
using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines methods and properties for an application bootstrapper used to create a simple dependency injection container.
/// </summary>
/// <remarks>This step of the application life cycle should only load the bare minimum of dependencies and configuration required to bootstrap the application.</remarks>
public partial class AppBootStrapper
{
    /// <summary>
    /// Gets a collection of injectors that will be used to bootstrap the application.
    /// </summary>
    protected List<IBootStrapInjector> Injectors { get; init; }
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
    public AppBootStrapper(string[] args)
    {
        // Start a new stopwatch to measure the bootstrapping time.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Discover the bootstrapping injectors.
        this.Injectors = this.DiscoverInjectors();

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
    /// Discovers bootstrapping injectors within all assemblies in the current app domain.
    /// </summary>
    /// <returns>A list of bootstrapping injectors.</returns>
    protected virtual List<IBootStrapInjector> DiscoverInjectors()
    {
        // Create a list to store the injectors in.
        List<IBootStrapInjector> injectors = [];

        // Discover the bootstrapping injectors.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IBootStrapInjector).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Iterate over the types found.
        foreach (Type type in types)
        {
            try
            {
                // Attempt to create the injector.
                IBootStrapInjector injector = (IBootStrapInjector)Activator.CreateInstance(type)!;

                // Add the injector to the list.
                injectors.Add(injector);
            }
            catch (Exception ex)
            {
                // Log the error.
                Console.WriteLine($"Failed to create bootstrap injector {type.Name}: {ex.Message}");
            }
        }

        // Return and sort the injectors.
        return this.SortInjectors(injectors);
    }

    /// <summary>
    /// Sorts the provided list of bootstrapping injectors using topological sorting based on their dependencies.
    /// </summary>
    /// <param name="injectors">The list of bootstrapping injectors to sort.</param>
    /// <returns>A sorted list of bootstrapping injectors.</returns>
    protected List<IBootStrapInjector> SortInjectors(List<IBootStrapInjector> injectors)
    {
        // Create a dictionary to store the injectors by type.
        Dictionary<Type, IBootStrapInjector> injectorLookup = injectors.ToDictionary(m => m.GetType());

        // Create a list to store the sorted injectors.
        List<IBootStrapInjector> sorted = [];

        // Create sets to track visited and visiting injectors.
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        // Iterate over the injectors and perform topological sorting.
        foreach (IBootStrapInjector injector in injectors)
        {
            // Visit the injector.
            this.VisitInjector(injector, injectorLookup, visited, visiting, sorted);
        }

        // Return and initialize the sorted injectors.
        return sorted;
    }

    /// <summary>
    /// Visits a bootstrapping injector and its dependencies, performing topological sorting.
    /// </summary>
    /// <param name="injector">The injector being visited.</param>
    /// <param name="injectorLookup">The dictionary of injectors by type.</param>
    /// <param name="visited">The set of visited injectors.</param>
    /// <param name="visiting">The set of injectors currently being visited.</param>
    /// <param name="sorted">The list of sorted injectors.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected void VisitInjector(IBootStrapInjector injector, Dictionary<Type, IBootStrapInjector> injectorLookup, HashSet<Type> visited, HashSet<Type> visiting, List<IBootStrapInjector> sorted)
    {
        // Get the injector type.
        Type injectorType = injector.GetType();

        // Check if the injector has already been visited.
        if (visited.Contains(injectorType))
        {
            return;
        }

        // Check if the injector is currently being visited.
        if (visiting.Contains(injectorType))
        {
            throw new InvalidOperationException($"Circular dependency detected involving {injectorType.Name}");
        }

        // Add the injector to the visiting set.
        _ = visiting.Add(injectorType);

        // Iterate over the injector's dependencies.
        foreach (Type dependency in injector.Dependencies)
        {
            // Check if the dependency has not been found.
            if (!injectorLookup.TryGetValue(dependency, out IBootStrapInjector? depInjector))
            {
                // Throw an exception.
                throw new InvalidOperationException(
                    $"Injector {injectorType.Name} depends on {dependency.Name}, but it was not found.");
            }

            // Visit the dependency injector.
            this.VisitInjector(depInjector, injectorLookup, visited, visiting, sorted);
        }

        // Remove the injector from the visiting set.
        _ = visiting.Remove(injectorType);

        // Add the injector to the visited set.
        _ = visited.Add(injectorType);

        // Add the injector to the sorted list.
        sorted.Add(injector);
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

        // Iterate over the configuration injectors.
        foreach (IBootStrapConfigurationInjector injector in this.Injectors.OfType<IBootStrapConfigurationInjector>())
        {
            // Invoke the injector's OnGetConfiguration method.
            injector.OnGetConfiguration(builder);
        }

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

        // Iterate over the logging injectors.
        foreach (IBootStrapLoggingInjector injector in this.Injectors.OfType<IBootStrapLoggingInjector>())
        {
            // Invoke the injector's OnGetLoggerFactory method.
            injector.OnGetLoggerFactory(loggerFactory, this.Configuration!);
        }

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
        _ = services.RegisterInstance(args)
            .Keyed<string[]>("CommandLineArgs")
            .As<string[]>()
            .SingleInstance();

        // Iterate over the services injectors.
        foreach (IBootStrapServicesInjector injector in this.Injectors.OfType<IBootStrapServicesInjector>())
        {
            // Invoke the injector's OnGetServices method.
            injector.OnGetServices(services);
        }

        // Iterate over the application injectors.
        foreach(IBootStrapApplicationInjector injector in this.Injectors.OfType<IBootStrapApplicationInjector>())
        {
            // Invoke the injector's OnGetServices method.
            injector.OnGetServices(services);
        }

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

        // Iterate over the services injectors.
        foreach (IBootStrapServicesInjector injector in this.Injectors.OfType<IBootStrapServicesInjector>())
        {
            // Invoke the injector's OnGetServices method.
            injector.OnConfigureServices(provider);
        }

        // Iterate over the application injectors.
        foreach (IBootStrapApplicationInjector injector in this.Injectors.OfType<IBootStrapApplicationInjector>())
        {
            // Invoke the injector's OnGetServices method.
            injector.OnConfigureServices(provider);
        }

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
            this.Logger.LogInformation("Bootstrapping completed, it took {time} ms. Injectors: {injectors}", stopwatch.ElapsedMilliseconds, this.Injectors.Count);
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