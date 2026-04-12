using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application.Configuration;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Network.Sockets;
using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Reoria.Client.Core.Application;

public class ClientApplication : Game, IApplication
{    
    protected virtual ILogger<IApplication> Logger { get; init; }
    protected virtual List<IApplicationInjector> Injectors { get; init; }
    protected IConfiguration Configuration { get; init; }
    protected ILoggerFactory LoggerFactory { get; init; }
    protected ContainerBuilder ContainerServices { get; init; }
    protected IServiceProvider Provider { get; set; }
    private GraphicsDeviceManager GraphicsDeviceManager { get; set; }
    private SpriteBatch? SpriteBatch { get; set; }
    protected TimeSpan Accumulator { get; set; }
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 30.0);
    protected int MaxSteps { get; init; } = 5;
    protected int Steps { get; set; } = 0;
    protected ClientSocket Socket { get; set; }

    public ClientApplication(ILogger<IApplication> logger, [KeyFilter("CommandLineArgs")] string[] args)
    {
        // Store the logger and report the initialization.
        this.Logger = logger;
        this.Logger.LogInformation("Initializing server application...");

        // Discover the application injectors.
        this.Injectors = this.DiscoverInjectors();

        // Get the configuration instance.
        this.Configuration = this.GetConfiguration(args);

        // Get the logger factory and logger instances.
        this.LoggerFactory = this.GetLoggerFactory();
        this.Logger = this.GetLogger();

        // Get the service collection and service provider instances.
        this.ContainerServices = this.GetServices();

        this.GraphicsDeviceManager = new GraphicsDeviceManager(this);
        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    /// <summary>
    /// Discovers application injectors within all assemblies in the current app domain.
    /// </summary>
    /// <returns>A list of application injectors.</returns>
    protected virtual List<IApplicationInjector> DiscoverInjectors()
    {
        // Create a list to store the injectors in.
        List<IApplicationInjector> injectors = [];

        // Discover the application injectors.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IApplicationInjector).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Iterate over the types found.
        foreach (Type type in types)
        {
            try
            {
                // Attempt to create the injector.
                IApplicationInjector injector = (IApplicationInjector)Activator.CreateInstance(type)!;

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
    /// Sorts the provided list of application injectors using topological sorting based on their dependencies.
    /// </summary>
    /// <param name="injectors">The list of application injectors to sort.</param>
    /// <returns>A sorted list of application injectors.</returns>
    protected List<IApplicationInjector> SortInjectors(List<IApplicationInjector> injectors)
    {
        // Create a dictionary to store the injectors by type.
        Dictionary<Type, IApplicationInjector> injectorLookup = injectors.ToDictionary(m => m.GetType());

        // Create a list to store the sorted injectors.
        List<IApplicationInjector> sorted = [];

        // Create sets to track visited and visiting injectors.
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        // Iterate over the injectors and perform topological sorting.
        foreach (IApplicationInjector injector in injectors)
        {
            // Visit the injector.
            this.VisitInjector(injector, injectorLookup, visited, visiting, sorted);
        }

        // Return and initialize the sorted injectors.
        return sorted;
    }

    /// <summary>
    /// Visits a application injector and its dependencies, performing topological sorting.
    /// </summary>
    /// <param name="injector">The injector being visited.</param>
    /// <param name="injectorLookup">The dictionary of injectors by type.</param>
    /// <param name="visited">The set of visited injectors.</param>
    /// <param name="visiting">The set of injectors currently being visited.</param>
    /// <param name="sorted">The list of sorted injectors.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected void VisitInjector(IApplicationInjector injector, Dictionary<Type, IApplicationInjector> injectorLookup, HashSet<Type> visited, HashSet<Type> visiting, List<IApplicationInjector> sorted)
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
            if (!injectorLookup.TryGetValue(dependency, out IApplicationInjector? depInjector))
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

    protected virtual IConfiguration GetConfiguration(string[] args)
    {
        AppConfigurationBuilder builder = new();

        _ = builder.AddConfigurationSource("appsettings.json", false, true);
        _ = builder.AddConfigurationSource("appsettings.logging.json", true, true);
        _ = builder.AddConfigurationSource("appsettings.serilog.json", true, true);

        foreach (IApplicationConfigurationInjector injector in this.Injectors.OfType<IApplicationConfigurationInjector>())
        {
            injector.OnGetConfiguration(builder);
        }

        _ = builder.AddCommandLine(args);

        return builder.Build() ?? throw new InvalidOperationException("Failed to build configuration.");
    }

    protected virtual ILoggerFactory GetLoggerFactory()
    {
        LoggerFactory loggerFactory = new();

        Log.CloseAndFlush();

        Log.Logger = this.Configuration is not null
            ? new LoggerConfiguration()
                .ReadFrom.Configuration(this.Configuration)
                .CreateLogger()
            : new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

        loggerFactory.AddProvider(new SerilogLoggerProvider(Log.Logger));

        foreach (IApplicationLoggingInjector injector in this.Injectors.OfType<IApplicationLoggingInjector>())
        {
            injector.OnGetLoggerFactory(loggerFactory, this.Configuration!);
        }

        return loggerFactory;
    }

    protected ILogger<IApplication> GetLogger()
        // Use the logger factory to create a logger for the application.
        => this.LoggerFactory.CreateLogger<IApplication>();

    protected virtual ContainerBuilder GetServices()
    {
        ContainerBuilder services = new();

        _ = services.RegisterInstance(this.Configuration)
            .As<IConfiguration>()
            .SingleInstance();
        _ = services.RegisterInstance(this.LoggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();
        _ = services.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        foreach (IApplicationServicesInjector injector in this.Injectors.OfType<IApplicationServicesInjector>())
        {
            injector.OnGetServices(services);
        }

        return services;
    }

    /// <inheritdoc />
    protected override void Initialize()
    {
        // Registe the graphics device manager.
        _ = this.ContainerServices.RegisterInstance<GraphicsDeviceManager>(this.GraphicsDeviceManager)
            .Keyed<GraphicsDeviceManager>("GraphicsDeviceManager")
            .As<GraphicsDeviceManager>()
            .SingleInstance();

        // Register the graphics device.
        _ = this.ContainerServices.RegisterInstance<GraphicsDevice>(this.GraphicsDevice)
            .Keyed<GraphicsDevice>("GraphicsDevice")
            .As<GraphicsDevice>()
            .SingleInstance();

        // Register the content manager.
        _ = this.ContainerServices.RegisterInstance<ContentManager>(this.Content)
            .Keyed<ContentManager>("ContentManager")
            .As<ContentManager>()
            .SingleInstance();

        // Call the base method.
        base.Initialize();
    }

    /// <inheritdoc />
    protected override void LoadContent()
    {
        // Create the sprite batch.
        this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

        // Register the sprite batch.
        _ = this.ContainerServices.RegisterInstance<SpriteBatch>(this.SpriteBatch)
            .Keyed<SpriteBatch>("SpriteBatch")
            .As<SpriteBatch>()
            .SingleInstance();

        // Call the base method.
        base.LoadContent();
    }

    /// <inheritdoc />
    protected override void BeginRun()
    {
        // Get the service provider.
        this.Provider = this.GetServiceProvider();

        // Get the server network socket.
        this.Socket = this.Provider.GetRequiredService<ClientSocket>();

        // Call the base method.
        base.BeginRun();
    }

    protected virtual IServiceProvider GetServiceProvider()
    {
        IContainer container = this.ContainerServices.Build();

        AutofacServiceProvider provider = new(container);

        foreach (IApplicationServicesInjector injector in this.Injectors.OfType<IApplicationServicesInjector>())
        {
            injector.OnConfigureServices(provider);
        }

        return provider;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
#if !IOS
            this.Exit();
#endif
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            if (!this.Socket.IsRunning)
            {
                this.Socket.Start();
                if (!this.Socket.Connect("127.0.0.1", 7234))
                {
#if !IOS
                    this.Exit();
#endif
                }
            }
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.BigButton == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Back))
        {
            if (this.Socket.IsRunning)
            {
                this.Socket.Stop();
            }
        }

        this.Socket.Update();

        this.Accumulator += gameTime.ElapsedGameTime;

        this.VariableUpdate(gameTime);

        while (this.Accumulator >= this.FixedStep && this.Steps < this.MaxSteps)
        {
            GameTime fixedGameTime = new(gameTime.TotalGameTime, this.FixedStep);
            this.FixedUpdate(fixedGameTime);

            this.Accumulator -= this.FixedStep;
            this.Steps++;
        }

        this.Steps = 0;

        base.Update(gameTime);
    }

    protected virtual void VariableUpdate(GameTime gameTime)
    {

    }

    protected virtual void FixedUpdate(GameTime gameTime)
    {

    }

    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
