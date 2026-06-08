using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using System.Reflection;

namespace Reoria.Engine.Application.Services.Interfaces;

public interface IInjectorService
{
    /// <summary>
    /// Adds an assembly to the assembly cache for injector discovery.
    /// </summary>
    /// <param name="assembly">The assembly to add.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService AddAssembly(Assembly assembly);

    /// <summary>
    /// Removes an assembly from the assembly cache.
    /// </summary>
    /// <param name="assembly">The assembly to remove.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService RemoveAssembly(Assembly assembly);

    /// <summary>
    /// Adds multiple assemblies to the assembly cache.
    /// </summary>
    /// <param name="assemblies">The assemblies to add.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService AddAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>
    /// Removes multiple assemblies from the assembly cache.
    /// </summary>
    /// <param name="assemblies">The assemblies to remove.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService RemoveAssemblies(IEnumerable<Assembly> assemblies);

    /// <summary>
    /// Clears all assemblies from the assembly cache.
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService ClearAssemblies();

    /// <summary>
    /// Sets the platform filter for injector discovery.
    /// </summary>
    /// <param name="platform">The platform to filter by.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService SetPlatform(Platform platform);

    /// <summary>
    /// Sets the service provider for resolving injectors via dependency injection.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for DI resolution.</param>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService SetServiceProvider(IServiceProvider serviceProvider);

    /// <summary>
    /// Executes all injectors of the specified type with the provided action.
    /// </summary>
    /// <typeparam name="TInjectorType">The type of injector to execute.</typeparam>
    /// <param name="action">The action to execute on each injector.</param>
    void ExecuteInjectors<TInjectorType>(Action<TInjectorType> action) where TInjectorType : IInjector;

    /// <summary>
    /// Clears the injector cache, forcing re-discovery on next execution.
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService ClearInjectorCache();

    /// <summary>
    /// Clears all caches (assembly and injector).
    /// </summary>
    /// <returns>The service instance for method chaining.</returns>
    IInjectorService ClearAllCaches();
}