using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Application.Services.Interfaces;
using System.Reflection;

namespace Reoria.Engine.Application.Services;

/// <summary>
/// Centralized service for discovering, caching, and executing phase participants across the application.
/// </summary>
/// <remarks>
/// This service provides a performant way to manage phase participant lifecycle with:
/// - Assembly-level caching to avoid repeated reflection overhead
/// - Type-level caching for discovered phase participants
/// - Topological sorting based on phase participant dependencies
/// - Platform filtering for cross-platform compatibility
/// - Fluent API pattern for method chaining
/// 
/// The service is particularly important for game loop phase participants that execute every frame,
/// as the caching ensures zero reflection overhead during runtime execution.
/// 
/// Example usage with method chaining:
/// <code>
/// phaseService
///     .SetPlatform(Platform.Windows)
///     .AddAssembly(myPluginAssembly)
///     .ExecutePhase&lt;IBootstrapConfiguration&gt;(phase => phase.OnBuildConfiguration(builder));
/// </code>
/// </remarks>
public class PhaseService : IPhaseService
{
    /// <summary>
    /// Cache of assemblies to scan for phase participant types. Initialized with all currently loaded assemblies.
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
    /// Cache of discovered and instantiated phase participants keyed by their interface type.
    /// </summary>
    /// <remarks>
    /// Each entry stores a <see cref="List{TPhaseType}"/> where T is the phase participant interface.
    /// This cache ensures that reflection-based discovery only happens once per phase participant type.
    /// Critical for performance in game loops where phase participants execute 60+ times per second.
    /// </remarks>
    protected Dictionary<Type, object> PhaseCache = [];

    /// <summary>
    /// The platform filter for phase participant discovery. Only phase participants matching this platform will be loaded.
    /// </summary>
    protected Platform Platform = Platform.All;

    /// <summary>
    /// The service provider for resolving phase participants via dependency injection.
    /// </summary>
    /// <remarks>
    /// When set, phase participants will be resolved from the DI container instead of being created via reflection.
    /// This is used for phase participants that run after the application is initialized (e.g., game loop phase participants).
    /// Bootstrap phase participants are still created via reflection since they run before the DI container is fully built.
    /// </remarks>
    protected IServiceProvider? ServiceProvider = null;

    /// <inheritdoc />
    public virtual IPhaseService AddAssembly(Assembly assembly)
    {
        this.AssemblyCache.Add(assembly);

        // Clear type cache when assemblies are added to ensure fresh discovery
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService RemoveAssembly(Assembly assembly)
    {
        _ = this.AssemblyCache.Remove(assembly);
        _ = this.AssemblyTypeCache.Remove(assembly);

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService AddAssemblies(IEnumerable<Assembly> assemblies)
    {
        this.AssemblyCache.AddRange(assemblies);
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService RemoveAssemblies(IEnumerable<Assembly> assemblies)
    {
        _ = this.AssemblyCache.RemoveAll(a => assemblies.Contains(a));
        foreach (Assembly assembly in assemblies)
        {
            _ = this.AssemblyTypeCache.Remove(assembly);
        }

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService ClearAssemblies()
    {
        this.AssemblyCache.Clear();
        this.AssemblyTypeCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService SetPlatform(Platform platform)
    {
        if (this.Platform != platform)
        {
            this.Platform = platform;

            // Clear phase cache when platform changes as filtering may differ
            this.PhaseCache.Clear();
        }

        return this;
    }

    /// <summary>
    /// Sets the service provider for resolving phase participants via dependency injection.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for DI resolution.</param>
    /// <returns>The service instance for method chaining.</returns>
    /// <remarks>
    /// When a service provider is set, phase participants will be resolved from the DI container
    /// instead of being created via reflection. This should be called after the DI container
    /// is built, typically during application startup.
    /// </remarks>
    public virtual IPhaseService SetServiceProvider(IServiceProvider serviceProvider)
    {
        this.ServiceProvider ??= serviceProvider;

        return this;
    }

    /// <inheritdoc />
    public virtual void ExecutePhase<TPhaseType>(Action<TPhaseType> action) where TPhaseType : IPhaseParticipant
    {
        List<TPhaseType> phases = this.GetCachedPhases<TPhaseType>();

        // Use foreach for optimal performance with List<T>
        foreach (TPhaseType phase in phases)
        {
            action(phase);
        }
    }

    /// <summary>
    /// Retrieves cached phase participants of the specified type, discovering them if not already cached.
    /// </summary>
    /// <typeparam name="TPhaseType">The type of phase participant to retrieve.</typeparam>
    /// <returns>A cached list of phase participants of the specified type.</returns>
    /// <remarks>
    /// This method is the core of the caching strategy. It ensures that:
    /// - Reflection-based discovery happens only once per phase participant type
    /// - Subsequent calls return the cached list with zero overhead
    /// - Platform filtering is applied during discovery, not retrieval
    /// </remarks>
    protected virtual List<TPhaseType> GetCachedPhases<TPhaseType>() where TPhaseType : IPhaseParticipant
    {
        Type phaseType = typeof(TPhaseType);

        if (!this.PhaseCache.TryGetValue(phaseType, out object? value))
        {
            value = this.DiscoverPhases<TPhaseType>();
            this.PhaseCache.Add(phaseType, value);
        }

        return (List<TPhaseType>)value;
    }

    /// <summary>
    /// Discovers and instantiates all phase participants of the specified type from cached assemblies.
    /// </summary>
    /// <typeparam name="TPhaseType">The type of phase participant to discover.</typeparam>
    /// <returns>A sorted list of discovered and instantiated phase participants.</returns>
    /// <remarks>
    /// Performance optimizations:
    /// - Uses assembly type cache to avoid repeated GetTypes() calls
    /// - Filters by interface before instantiation to reduce Activator calls
    /// - Applies platform filtering early to skip incompatible phase participants
    /// - Uses <see cref="List{TPhaseType}"/> with capacity pre-allocation for better memory performance
    /// </remarks>
    protected virtual List<TPhaseType> DiscoverPhases<TPhaseType>() where TPhaseType : IPhaseParticipant
    {
        List<TPhaseType> phases = [];
        Type interfaceType = typeof(TPhaseType);

        // Use DI resolution if service provider is available, otherwise use reflection
        if (this.ServiceProvider is not null)
        {
            IEnumerable<TPhaseType> services = this.ServiceProvider.GetServices<TPhaseType>();

            phases.AddRange(services);

            return SortPhases(phases);
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
                    TPhaseType phase = (TPhaseType)Activator.CreateInstance(type)!;

                    // Platform filtering - skip incompatible phase participants early
                    if (!phase.Platform.Matches(this.Platform))
                    {
                        continue;
                    }

                    phases.Add(phase);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create phase participant {type.Name}: {ex.Message}");
                }
            }
        }

        return SortPhases(phases);
    }

    /// <summary>
    /// Sorts the provided list of phase participants using topological sorting based on their dependencies.
    /// </summary>
    /// <typeparam name="TPhaseType">The type of phase participant to sort.</typeparam>
    /// <param name="phases">The list of phase participants to sort.</param>
    /// <returns>A sorted list of phase participants in dependency order.</returns>
    /// <remarks>
    /// Uses depth-first search with cycle detection to ensure phase participants are executed
    /// in the correct order based on their declared dependencies.
    /// </remarks>
    protected static List<TPhaseType> SortPhases<TPhaseType>(List<TPhaseType> phases) where TPhaseType : IPhaseParticipant
    {
        // Pre-allocate capacity for better performance
        Dictionary<Type, TPhaseType> phaseLookup = new(phases.Count);
        foreach (TPhaseType phase in phases)
        {
            phaseLookup[phase.GetType()] = phase;
        }

        List<TPhaseType> sorted = new(phases.Count);
        HashSet<Type> visited = [];
        HashSet<Type> visiting = [];

        foreach (TPhaseType phase in phases)
        {
            VisitPhase(phase, phaseLookup, visited, visiting, sorted);
        }

        return sorted;
    }

    /// <summary>
    /// Visits a phase participant and its dependencies recursively to perform topological sorting.
    /// </summary>
    /// <typeparam name="TPhaseType">The type of phase participant being visited.</typeparam>
    /// <param name="phase">The phase participant being visited.</param>
    /// <param name="phaseLookup">The dictionary of phase participants by type for dependency lookup.</param>
    /// <param name="visited">The set of already visited phase participants to avoid re-processing.</param>
    /// <param name="visiting">The set of phase participants currently being visited for cycle detection.</param>
    /// <param name="sorted">The list to add sorted phase participants to.</param>
    /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
    protected static void VisitPhase<TPhaseType>(TPhaseType phase, Dictionary<Type, TPhaseType> phaseLookup, HashSet<Type> visited, HashSet<Type> visiting, List<TPhaseType> sorted) where TPhaseType : IPhaseParticipant
    {
        Type phaseType = phase.GetType();

        if (visited.Contains(phaseType))
        {
            return;
        }

        if (visiting.Contains(phaseType))
        {
            throw new InvalidOperationException($"Circular dependency detected involving {phaseType.Name}");
        }

        _ = visiting.Add(phaseType);

        foreach (Type dependency in phase.Dependencies)
        {
            if (!phaseLookup.TryGetValue(dependency, out TPhaseType? depPhase))
            {
                throw new InvalidOperationException(
                    $"Phase participant {phaseType.Name} depends on {dependency.Name}, but it was not found.");
            }

            VisitPhase(depPhase, phaseLookup, visited, visiting, sorted);
        }

        _ = visiting.Remove(phaseType);
        _ = visited.Add(phaseType);
        sorted.Add(phase);
    }

    /// <inheritdoc />
    public virtual IPhaseService ClearPhaseCache()
    {
        this.PhaseCache.Clear();

        return this;
    }

    /// <inheritdoc />
    public virtual IPhaseService ClearAllCaches()
    {
        this.AssemblyCache.Clear();
        this.AssemblyTypeCache.Clear();
        this.PhaseCache.Clear();
        this.ServiceProvider = null;

        return this;
    }
}