using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Reoria.Engine.Core.Reflection;

/// <summary>
/// Provides cached assembly and type discovery functionality.
/// </summary>
public static class TypeDiscoveryHelper
{
    private static readonly Lazy<Assembly[]> _cachedAssemblies = new(() =>
        [.. AppDomain.CurrentDomain.GetAssemblies()]);

    private static readonly ConcurrentDictionary<Type, Type[]> _typeCache = new();

    /// <summary>
    /// Gets all cached assemblies from the current AppDomain.
    /// </summary>
    /// <returns>Array of assemblies.</returns>
    public static Assembly[] GetCachedAssemblies() => _cachedAssemblies.Value;

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
            : _typeCache.GetOrAdd(interfaceType, type =>
        {
            List<Type> results = [];

            foreach (Assembly assembly in _cachedAssemblies.Value)
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
    /// Clears the type cache. Useful for testing or when assemblies are dynamically loaded.
    /// </summary>
    public static void ClearCache() => _typeCache.Clear();
}