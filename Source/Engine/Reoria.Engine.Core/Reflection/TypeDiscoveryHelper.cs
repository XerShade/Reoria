using System.Collections.Concurrent;
using System.Reflection;

namespace Reoria.Engine.Core.Reflection;

/// <summary>
/// Provides cached assembly and type discovery functionality with optimized assembly filtering.
/// </summary>
public static class TypeDiscoveryHelper
{
    /// <summary>
    /// Cached assemblies from the current AppDomain, excluding system assemblies.
    /// </summary>
    private static readonly Lazy<Assembly[]> CachedAssemblies = new(() =>
        FilterAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

    /// <summary>
    /// Type cache per interface type for performance.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, Type[]> TypeCache = new();

    /// <summary>
    /// Array of prefixes to ignore when filtering assemblies.
    /// </summary>
    private static readonly string[] IgnoredAssemblyPrefixes =
    [
        "System.",
        "Microsoft.",
        "netstandard",
        "mscorlib",
        "WindowsBase"
    ];

    /// <summary>
    /// Gets all cached assemblies from the current AppDomain, excluding system assemblies.
    /// </summary>
    /// <returns>Array of filtered assemblies.</returns>
    public static Assembly[] GetCachedAssemblies() => CachedAssemblies.Value;

    /// <summary>
    /// Gets all concrete types that implement the specified interface type.
    /// Results are cached per interface type for performance.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to search for.</typeparam>
    /// <returns>Array of concrete types implementing the interface.</returns>
    public static Type[] GetConcreteTypesImplementingInterface<TInterface>()
        where TInterface : class => GetConcreteTypesImplementingInterface(typeof(TInterface));

    /// <summary>
    /// Gets all concrete types that implement the specified interface type.
    /// Results are cached per interface type for performance.
    /// </summary>
    /// <param name="interfaceType">The interface type to search for.</param>
    /// <returns>Array of concrete types implementing the interface.</returns>
    public static Type[] GetConcreteTypesImplementingInterface(Type interfaceType)
        => !interfaceType.IsInterface
            ? throw new ArgumentException($"Type {interfaceType.Name} must be an interface", nameof(interfaceType))
            : TypeCache.GetOrAdd(interfaceType, type =>
        {
            List<Type> results = [];

            foreach (Assembly assembly in CachedAssemblies.Value)
            {
                try
                {
                    Type[] types;

                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(t => t != null).ToArray()!;
                    }

                    foreach (Type t in types)
                    {
                        if (t == null)
                        {
                            continue;
                        }

                        if (type.IsAssignableFrom(t) &&
                            !t.IsInterface &&
                            !t.IsAbstract)
                        {
                            results.Add(t);
                        }
                    }
                }
                catch
                {
                    // ignore broken assemblies safely
                }
            }

            return [.. results];
        });

    /// <summary>
    /// Filters out system assemblies to improve discovery performance.
    /// </summary>
    /// <param name="assemblies">All assemblies to filter.</param>
    /// <returns>Filtered array of assemblies excluding system assemblies.</returns>
    private static Assembly[] FilterAssemblies(Assembly[] assemblies) 
        => [.. assemblies.Where(assembly =>
            {
                string? name = assembly.GetName().Name;

                // Skip dynamic assemblies
                if (assembly.IsDynamic)
                {
                    return false;
                }

                // Skip system assemblies for performance
                return name == null || !IgnoredAssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)); 
            })];

    /// <summary>
    /// Clears the type cache. Useful for testing or when assemblies are dynamically loaded.
    /// </summary>
    public static void ClearCache() => TypeCache.Clear();
}