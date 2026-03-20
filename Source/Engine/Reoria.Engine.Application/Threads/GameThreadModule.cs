using Autofac;
using Reoria.Engine.Application.Modules;
using System.Reflection;

namespace Reoria.Engine.Application.Threads;

public class GameThreadModule : IApplicationServicesModule
{
    /// <summary>
    /// Indicates if the main thread has been registered.
    /// </summary>
    protected bool IsMainThreadRegistered = false;

    /// <inheritdoc />
    public string Name
        => "Game Threads Module";

    /// <inheritdoc />
    public string Description 
        => "Discovers and registers game threads with the dependency injection container.";

    /// <inheritdoc />
    public Type[] Dependencies
        => [];

    /// <inheritdoc />
    public void OnGetServices(ContainerBuilder services)
    {
        // Discover the game threads.
        Type[] threads = [.. this.DiscoverThreads()];

        // Iterate through the game threads and register them.
        foreach(Type thread in threads)
        {
            // Register the game thread.
            _ = services.RegisterType(thread)
                .Keyed<IGameThread>(thread.Name)
                .As<IGameThread>().AsImplementedInterfaces()
                .SingleInstance();

            // Tempoary: Register the first thread as the main thread.
            if(!this.IsMainThreadRegistered && typeof(IGameThread).IsAssignableFrom(thread))
            {
                // Register the main thread.
                _ = services.RegisterType(thread)
                    .Keyed<IGameThread>("MainThread")
                    .As<IGameThread>().AsImplementedInterfaces()
                    .SingleInstance();

                // Indicate that the main thread has been registered.
                this.IsMainThreadRegistered = true;
            }
        }
    }

    /// <summary>
    /// Discovers game threads within all assemblies in the current app domain.
    /// </summary>
    /// <returns>A list of game threads.</returns>
    protected Type[] DiscoverThreads()
    {
        // Create a list to store the modules in.
        List<IGameThread> threads = [];

        // Discover the game threads.
        Assembly[] assemblies = [.. AppDomain.CurrentDomain.GetAssemblies()];
        Type[] types = [.. assemblies
                .SelectMany(a =>{ try { return a.GetTypes(); } catch { return []; }})
                .Where(t => typeof(IGameThread).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)];

        // Return the modules.
        return types;
    }

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    {
        // Game threads do not require configuration at this time.
    }
}