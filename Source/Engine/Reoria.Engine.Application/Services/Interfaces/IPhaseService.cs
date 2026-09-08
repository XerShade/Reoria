using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using System.Reflection;

namespace Reoria.Engine.Application.Services.Interfaces;

/// <summary>
/// Service for discovering, caching, and executing phase participants across the application.
/// </summary>
public interface IPhaseService
{
    /// <summary>
    /// Adds an assembly to the assembly cache for phase participant discovery.
    /// </summary>
    /// <param name="assembly">The assembly to add.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService AddAssembly(Assembly assembly);

    /// <summary>
    /// Removes an assembly from the assembly cache.
    /// </summary>
    /// <param name="assembly">The assembly to remove.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService RemoveAssembly(Assembly assembly);

    /// <summary>
    /// Adds multiple assemblies to the assembly cache.
    /// </summary>
    /// <param name="assemblies">The assemblies to add.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService AddAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>
    /// Removes multiple assemblies from the assembly cache.
    /// </summary>
    /// <param name="assemblies">The assemblies to remove.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService RemoveAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>
    /// Clears all assemblies from the assembly cache.
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService ClearAssemblies();

    /// <summary>
    /// Sets the platform filter for phase participant discovery.
    /// </summary>
    /// <param name="platform">The platform to filter by.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService SetPlatform(Platform platform);

    /// <summary>
    /// Sets the service provider for resolving phase participants via dependency injection.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for DI resolution.</param>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService SetServiceProvider(IServiceProvider serviceProvider);

    /// <summary>
    /// Executes all phase participants of the specified type with the provided action.
    /// </summary>
    /// <typeparam name="TPhaseType">The type of phase participant to execute.</typeparam>
    /// <param name="action">The action to execute on each phase participant.</param>
    void ExecutePhase<TPhaseType>(Action<TPhaseType> action) where TPhaseType : IPhaseParticipant;

    /// <summary>
    /// Clears the phase participant cache, forcing re-discovery on next execution.
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService ClearPhaseCache();

    /// <summary>
    /// Clears all caches (assembly and phase participant).
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IPhaseService ClearAllCaches();
}