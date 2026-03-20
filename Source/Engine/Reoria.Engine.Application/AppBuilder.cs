using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Modules;
using Reoria.Engine.Application.Threads;
using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines methods and properties for an application builder.
/// </summary>
public partial class AppBuilder
{
    /// <summary>
    /// Gets an instance of <see cref="AppBootStrapper"/> that can be used to bootstrap the application.
    /// </summary>
    protected AppBootStrapper BootStrapper { get; init; }
    /// <summary>
    /// Gets a collection of modules that will be used to bootstrap the application.
    /// </summary>
    protected List<IApplicationModule> Modules { get; init; }
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
    protected ILogger<AppBuilder> Logger { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="ContainerBuilder"/> that can be used to register bootstrap services.
    /// </summary>
    protected ContainerBuilder Services { get; init; }
    /// <summary>
    /// Gets an instance of <see cref="IServiceProvider"/> that can be used to resolve bootstrap services.
    /// </summary>
    protected IServiceProvider Provider { get; init; }

    /// <summary>
    /// Constructs a new instance of the <see cref="AppBuilder"/> class.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public AppBuilder(string[] args)
    {
        // Initialize the bootstrapper and start logging.
        this.BootStrapper = new AppBootStrapper(args);
        this.Logger = this.BootStrapper.GetLogger<AppBuilder>();
        this.Logger.LogInformation("Starting game engine creation and initialization.");

        // Start a new stopwatch to measure the application time.
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Discover the application modules.
        this.Modules = this.DiscoverModules();

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.GetLogger();

        // Get the service collection and service provider instances.
        this.Services = this.GetServices();
        this.Provider = this.GetServiceProvider();

        // Stop the stopwatch to measure the application time.
        stopwatch.Stop();
    }

    /// <summary>
    /// Discovers application modules within all assemblies in the current app domain.
    /// </summary>
    /// <returns>A list of application modules.</returns>
    protected virtual List<IApplicationModule> DiscoverModules()
    {
        // Create a list to store the modules in.
        List<IApplicationModule> modules = [];

        // Discover the application modules.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IApplicationModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Iterate over the types found.
        foreach (Type type in types)
        {
            try
            {
                // Attempt to create the module.
                IApplicationModule module = (IApplicationModule)Activator.CreateInstance(type)!;

                // Add the module to the list.
                modules.Add(module);
            }
            catch (Exception ex)
            {
                // Log the error.
                Console.WriteLine($"Failed to create bootstrap module {type.Name}: {ex.Message}");
            }
        }

        // Return and sort the modules.
        return this.SortModules(modules);
    }

    /// <summary>
    /// Sorts the provided list of application modules using topological sorting based on their dependencies.
    /// </summary>
    /// <param name="modules">The list of application modules to sort.</param>
    /// <returns>A sorted list of application modules.</returns>
    protected List<IApplicationModule> SortModules(List<IApplicationModule> modules)
    {
        // Create a dictionary to store the modules by type.
        Dictionary<Type, IApplicationModule> moduleLookup = modules.ToDictionary(m => m.GetType());

        // Create a list to store the sorted modules.
        List<IApplicationModule> sorted = [];

        // Create sets to track visited and visiting modules.
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        // Iterate over the modules and perform topological sorting.
        foreach (IApplicationModule module in modules)
        {
            // Visit the module.
            this.VisitModule(module, moduleLookup, visited, visiting, sorted);
        }

        // Return and initialize the sorted modules.
        return sorted;
    }

    /// <summary>
    /// Visits a application module and its dependencies, performing topological sorting.
    /// </summary>
    /// <param name="module">The module being visited.</param>
    /// <param name="moduleLookup">The dictionary of modules by type.</param>
    /// <param name="visited">The set of visited modules.</param>
    /// <param name="visiting">The set of modules currently being visited.</param>
    /// <param name="sorted">The list of sorted modules.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected void VisitModule(IApplicationModule module, Dictionary<Type, IApplicationModule> moduleLookup, HashSet<Type> visited, HashSet<Type> visiting, List<IApplicationModule> sorted)
    {
        // Get the module type.
        Type moduleType = module.GetType();

        // Check if the module has already been visited.
        if (visited.Contains(moduleType))
        {
            return;
        }

        // Check if the module is currently being visited.
        if (visiting.Contains(moduleType))
        {
            throw new InvalidOperationException($"Circular dependency detected involving {moduleType.Name}");
        }

        // Add the module to the visiting set.
        _ = visiting.Add(moduleType);

        // Iterate over the module's dependencies.
        foreach (Type dependency in module.Dependencies)
        {
            // Check if the dependency has not been found.
            if (!moduleLookup.TryGetValue(dependency, out IApplicationModule? depModule))
            {
                // Throw an exception.
                throw new InvalidOperationException(
                    $"Module {moduleType.Name} depends on {dependency.Name}, but it was not found.");
            }

            // Visit the dependency module.
            this.VisitModule(depModule, moduleLookup, visited, visiting, sorted);
        }

        // Remove the module from the visiting set.
        _ = visiting.Remove(moduleType);

        // Add the module to the visited set.
        _ = visited.Add(moduleType);

        // Add the module to the sorted list.
        sorted.Add(module);
    }

    /// <summary>
    /// Creates and builds the <see cref="IConfiguration"/> instance for the application.
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

        // Iterate over the configuration modules.
        foreach (IApplicationConfigurationModule module in this.Modules.OfType<IApplicationConfigurationModule>())
        {
            // Invoke the module's OnGetConfiguration method.
            module.OnGetConfiguration(builder);
        }

        // Add the command-line arguments.
        _ = builder.AddCommandLine(args);

        // Build the configuration and return it.
        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }

    /// <summary>
    /// Creates and builds the <see cref="ILoggerFactory"/> instance for the application.
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

        // Iterate over the logging modules.
        foreach (IApplicationLoggingModule module in this.Modules.OfType<IApplicationLoggingModule>())
        {
            // Invoke the module's OnGetLoggerFactory method.
            module.OnGetLoggerFactory(loggerFactory, this.Configuration!);
        }

        // Return the logger factory.
        return loggerFactory;
    }

    /// <summary>
    /// Creates and builds the <see cref="Microsoft.Extensions.Logging.ILogger"/> instance for the application.
    /// </summary>
    /// <returns>The built <see cref="Microsoft.Extensions.Logging.ILogger"/> instance.</returns>
    protected ILogger<AppBuilder> GetLogger()
        // Use the logger factory to create a logger for the application.
        => this.LoggerFactory.CreateLogger<AppBuilder>();

    /// <summary>
    /// Creates and builds the <see cref="ContainerBuilder"/> instance for the application.
    /// </summary>
    /// <returns>The built <see cref="ContainerBuilder"/> instance.</returns>
    protected virtual ContainerBuilder GetServices()
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

        // Iterate over the services modules.
        foreach (IApplicationServicesModule module in this.Modules.OfType<IApplicationServicesModule>())
        {
            // Invoke the module's OnGetServices method.
            module.OnGetServices(services);
        }

        // Return the container builder.
        return services;
    }

    /// <summary>
    /// Creates and builds the <see cref="IServiceProvider"/> instance for the application.
    /// </summary>
    /// <returns>The built <see cref="IServiceProvider"/> instance.</returns>
    protected virtual IServiceProvider GetServiceProvider()
    {
        // Build the autofac container.
        IContainer container = this.Services.Build();

        // Create a new autofac service provider.
        AutofacServiceProvider provider = new(container);

        // Iterate over the services modules.
        foreach (IApplicationServicesModule module in this.Modules.OfType<IApplicationServicesModule>())
        {
            // Invoke the module's OnGetServices method.
            module.OnConfigureServices(provider);
        }

        // Return the autofac service provider.
        return provider;
    }

    /// <summary>
    /// Builds and runs the main thread of the application.
    /// </summary>
    public virtual IGameThread Build()
    {
        // Attempt to get the main thread.
        return this.Provider.GetRequiredKeyedService<IGameThread>("MainThread") ?? throw new InvalidOperationException("Failed to get main thread.");
    }
}