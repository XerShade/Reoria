using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Core.Reflection;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for working with application injectors within an application.
/// </summary>
public static class ApplicationInjectorExtensions
{
    /// <summary>
    /// Discovers application injectors within all assemblies in the current app domain.
    /// </summary>
    /// <returns>A list of application injectors.</returns>
    public static List<IApplicationInjector> DiscoverInjectors(this IApplication application)
    {
        // Create a list to store the injectors in.
        List<IApplicationInjector> injectors = [.. application.Injectors];

        // Discover the application injectors.
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<IApplicationInjector>();

        // Iterate over the types found.
        foreach (Type type in types)
        {
            try
            {
                // Attempt to create the injector.
                IApplicationInjector injector = (IApplicationInjector)Activator.CreateInstance(type)!;

                // Check if the injector matches the application's platform.
                if (!injector.Platform.Matches(application.Platform))
                {
                    // Skip the injector.
                    continue;
                }

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
        return SortInjectors(injectors);
    }

    /// <summary>
    /// Sorts the provided list of application injectors using topological sorting based on their dependencies.
    /// </summary>
    /// <param name="injectors">The list of application injectors to sort.</param>
    /// <returns>A sorted list of application injectors.</returns>
    static List<IApplicationInjector> SortInjectors(List<IApplicationInjector> injectors)
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
            VisitInjector(injector, injectorLookup, visited, visiting, sorted);
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
    static void VisitInjector(IApplicationInjector injector, Dictionary<Type, IApplicationInjector> injectorLookup, HashSet<Type> visited, HashSet<Type> visiting, List<IApplicationInjector> sorted)
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
            VisitInjector(depInjector, injectorLookup, visited, visiting, sorted);
        }

        // Remove the injector from the visiting set.
        _ = visiting.Remove(injectorType);

        // Add the injector to the visited set.
        _ = visited.Add(injectorType);

        // Add the injector to the sorted list.
        sorted.Add(injector);
    }
}