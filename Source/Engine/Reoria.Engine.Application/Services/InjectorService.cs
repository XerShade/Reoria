using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Services.Interfaces;
using System.Reflection;

namespace Reoria.Engine.Application.Services;

/// <summary>
/// Centralized service for discovering, caching, and executing injectors across the application.
/// </summary>
/// <remarks>
/// This service provides a performant way to manage injector lifecycle with:
/// - Assembly-level caching to avoid repeated reflection overhead
/// - Type-level caching for discovered injectors
/// - Topological sorting based on injector dependencies
/// - Platform filtering for cross-platform compatibility
/// - Fluent API pattern for method chaining
/// 
/// The service is particularly important for game loop injectors that execute every frame,
/// as the caching ensures zero reflection overhead during runtime execution.
/// 
/// Example usage with method chaining:
/// <code>
/// injectorService
///     .SetPlatform(Platform.Windows)
///     .AddAssembly(myPluginAssembly)
///     .ExecuteInjectors&lt;IBootStrapConfigurationInjector&gt;(injector => injector.OnBuildConfiguration(builder));
/// </code>
/// </remarks>
public class InjectorService : IInjectorService
{
    /// <summary>
    /// Cache of assemblies to scan for injector types. Initialized with all currently loaded assemblies.
    /// </summary>
    /// <remarks>
    /// This cache avoids repeated calls to AppDomain.GetAssemblies() which can be expensive.
    /// Assemblies can be dynamically added or removed for plugin scenarios.
    /// </remarks>
    protected List<Assembly> AssemblyCache = [];

    /// <summary>
    /// Cache of assembly types to avoid repeated GetTypes() calls.
    /// </summary>
    /// <remarks>
    /// GetTypes() is expensive and can throw on some assemblies. Caching the results
    /// per assembly significantly improves discovery performance.
    /// </remarks>
    protected readonly Dictionary<Assembly, Type[]> AssemblyTypeCache = [];

    /// <summary>
    /// Cache of discovered and instantiated injectors keyed by their interface type.
    /// </summary>
    /// <remarks>
    /// Each entry stores a <see cref="List{TInjectorType}"/> where T is the injector interface.
    /// This cache ensures that reflection-based discovery only happens once per injector type.
    /// Critical for performance in game loops where injectors execute 60+ times per second.
    /// </remarks>
    protected Dictionary<Type, object> InjectorCache = [];

    /// <summary>
    /// The platform filter for injector discovery. Only injectors matching this platform will be loaded.
    /// </summary>
    protected Platform Platform = Platform.All;

    /// <summary>
    /// The service provider for resolving injectors via dependency injection.
    /// </summary>
    /// <remarks>
    /// When set, injectors will be resolved from the DI container instead of being created via reflection.
    /// This is used for injectors that run after the application is initialized (e.g., game loop injectors).
    /// Bootstrap and application lifecycle injectors are still created via reflection since they run
    /// before the DI container is fully built.
    /// </remarks>
    protected IServiceProvider? ServiceProvider = null;

    /// <inheritdoc />
    public virtual IInjectorService AddAssembly(Assembly assembly)
    {
        this.AssemblyCache.Add(assembly);

        // Clear type cache when assemblies are added to ensure fresh discovery
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService RemoveAssembly(Assembly assembly)
    {
        _ = this.AssemblyCache.Remove(assembly);
        _ = this.AssemblyTypeCache.Remove(assembly);

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService AddAssemblies(IEnumerable<Assembly> assemblies)
    {
        this.AssemblyCache.AddRange(assemblies);
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService RemoveAssemblies(IEnumerable<Assembly> assemblies)
    {
        _ = this.AssemblyCache.RemoveAll(a => assemblies.Contains(a));
        foreach (Assembly assembly in assemblies)
        {
            _ = this.AssemblyTypeCache.Remove(assembly);
        }

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService ClearAssemblies()
    {
        this.AssemblyCache.Clear();
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService SetPlatform(Platform platform)
    {
        if (this.Platform != platform)
        {
            this.Platform = platform;

            // Clear injector cache when platform changes as filtering may differ
            this.InjectorCache.Clear();
        }

        return this;
    }

    /// <summary>
    /// Sets the service provider for resolving injectors via dependency injection.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for DI resolution.</param>
    /// <returns>The service instance for method chaining.</returns>
    /// <remarks>
    /// When a service provider is set, injectors will be resolved from the DI container
    /// instead of being created via reflection. This should be called after the DI container
    /// is built, typically during application startup.
    /// </remarks>
    public virtual IInjectorService SetServiceProvider(IServiceProvider serviceProvider)
    {
        this.ServiceProvider ??= serviceProvider;

        return this;
    }

    /// <inheritdoc />
    public virtual void ExecuteInjectors<TInjectorType>(Action<TInjectorType> action) where TInjectorType : IInjector
    {
        List<TInjectorType> injectors = this.GetCachedInjectors<TInjectorType>();

        // Use foreach for optimal performance with List<T>
        foreach (TInjectorType injector in injectors)
        {
            action(injector);
        }
    }

    /// <summary>
    /// Retrieves cached injectors of the specified type, discovering them if not already cached.
    /// </summary>
    /// <typeparam name="TInjectorType">The type of injector to retrieve.</typeparam>
    /// <returns>A cached list of injectors of the specified type.</returns>
    /// <remarks>
    /// This method is the core of the caching strategy. It ensures that:
    /// - Reflection-based discovery happens only once per injector type
    /// - Subsequent calls return the cached list with zero overhead
    /// - Platform filtering is applied during discovery, not retrieval
    /// </remarks>
    protected virtual List<TInjectorType> GetCachedInjectors<TInjectorType>() where TInjectorType : IInjector
    {
        Type injectorType = typeof(TInjectorType);

        if (!this.InjectorCache.TryGetValue(injectorType, out object? value))
        {
            value = this.DiscoverInjectors<TInjectorType>();
            this.InjectorCache.Add(injectorType, value);
        }

        return (List<TInjectorType>)value;
    }

    /// <summary>
    /// Discovers and instantiates all injectors of the specified type from cached assemblies.
    /// </summary>
    /// <typeparam name="TInjectorType">The type of injector to discover.</typeparam>
    /// <returns>A sorted list of discovered and instantiated injectors.</returns>
    /// <remarks>
    /// Performance optimizations:
    /// - Uses assembly type cache to avoid repeated GetTypes() calls
    /// - Filters by interface before instantiation to reduce Activator calls
    /// - Applies platform filtering early to skip incompatible injectors
    /// - Uses <see cref="List{TInjectorType}"/> with capacity pre-allocation for better memory performance
    /// </remarks>
    protected virtual List<TInjectorType> DiscoverInjectors<TInjectorType>() where TInjectorType : IInjector
    {
        List<TInjectorType> injectors = [];
        Type interfaceType = typeof(TInjectorType);

        // Use DI resolution if service provider is available, otherwise use reflection
        if (this.ServiceProvider is not null)
        {
            IEnumerable<TInjectorType> services = this.ServiceProvider.GetServices<TInjectorType>();

            injectors.AddRange(services);

            return SortInjectors(injectors);
        }

        // Iterate over cached assemblies with type caching
        foreach (Assembly assembly in this.AssemblyCache)
        {
            // Get or cache types for this assembly
            if (!this.AssemblyTypeCache.TryGetValue(assembly, out Type[]? types))
            {
                try
                {
                    types = assembly.GetTypes();
                    this.AssemblyTypeCache[assembly] = types;
                }
                catch
                {
                    // Some assemblies may not allow reflection (e.g., dynamic assemblies)
                    this.AssemblyTypeCache[assembly] = [];
                    continue;
                }
            }

            // Filter and instantiate types from this assembly
            foreach (Type type in types)
            {
                // Fast path: skip if not assignable or is interface/abstract
                if (!interfaceType.IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                try
                {
                    TInjectorType injector = (TInjectorType)Activator.CreateInstance(type)!;

                    // Platform filtering - skip incompatible injectors early
                    if (!injector.Platform.Matches(this.Platform))
                    {
                        continue;
                    }

                    injectors.Add(injector);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create injector {type.Name}: {ex.Message}");
                }
            }
        }

        return SortInjectors(injectors);
    }

    /// <summary>
    /// Sorts the provided list of injectors using topological sorting based on their dependencies.
    /// </summary>
    /// <typeparam name="TInjectorType">The type of injector to sort.</typeparam>
    /// <param name="injectors">The list of injectors to sort.</param>
    /// <returns>A sorted list of injectors in dependency order.</returns>
    /// <remarks>
    /// Uses depth-first search with cycle detection to ensure injectors are executed
    /// in the correct order based on their declared dependencies.
    /// </remarks>
    protected static List<TInjectorType> SortInjectors<TInjectorType>(List<TInjectorType> injectors) where TInjectorType : IInjector
    {
        // Pre-allocate capacity for better performance
        Dictionary<Type, TInjectorType> injectorLookup = new(injectors.Count);
        foreach (TInjectorType injector in injectors)
        {
            injectorLookup[injector.GetType()] = injector;
        }

        List<TInjectorType> sorted = new(injectors.Count);
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        foreach (TInjectorType injector in injectors)
        {
            VisitInjector(injector, injectorLookup, visited, visiting, sorted);
        }

        return sorted;
    }

    /// <summary>
    /// Visits an injector and its dependencies recursively to perform topological sorting.
    /// </summary>
    /// <typeparam name="TInjectorType">The type of injector being visited.</typeparam>
    /// <param name="injector">The injector being visited.</param>
    /// <param name="injectorLookup">The dictionary of injectors by type for dependency lookup.</param>
    /// <param name="visited">The set of already visited injectors to avoid re-processing.</param>
    /// <param name="visiting">The set of injectors currently being visited for cycle detection.</param>
    /// <param name="sorted">The list to add sorted injectors to.</param>
    /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
    protected static void VisitInjector<TInjectorType>(TInjectorType injector, Dictionary<Type, TInjectorType> injectorLookup, HashSet<Type> visited, HashSet<Type> visiting, List<TInjectorType> sorted) where TInjectorType : IInjector
    {
        Type injectorType = injector.GetType();

        if (visited.Contains(injectorType))
        {
            return;
        }

        if (visiting.Contains(injectorType))
        {
            throw new InvalidOperationException($"Circular dependency detected involving {injectorType.Name}");
        }

        _ = visiting.Add(injectorType);

        foreach (Type dependency in injector.Dependencies)
        {
            if (!injectorLookup.TryGetValue(dependency, out TInjectorType? depInjector))
            {
                throw new InvalidOperationException(
                    $"Injector {injectorType.Name} depends on {dependency.Name}, but it was not found.");
            }

            VisitInjector(depInjector, injectorLookup, visited, visiting, sorted);
        }

        _ = visiting.Remove(injectorType);
        _ = visited.Add(injectorType);
        sorted.Add(injector);
    }

    /// <inheritdoc />
    public virtual IInjectorService ClearInjectorCache()
    {
        this.InjectorCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IInjectorService ClearAllCaches()
    {
        this.AssemblyCache.Clear();
        this.AssemblyTypeCache.Clear();
        this.InjectorCache.Clear();
        this.ServiceProvider = null;

        return this;
    }
}