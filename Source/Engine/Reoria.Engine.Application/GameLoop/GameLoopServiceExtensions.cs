using Autofac;
using Reoria.Engine.Application.GameLoop.Phases;

namespace Reoria.Engine.Application.GameLoop;

/// <summary>
/// Extension methods for registering game loop services in the DI container.
/// </summary>
public static class GameLoopServiceExtensions
{
    /// <summary>
    /// Registers the core game loop services (registry and factory) in the DI container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <returns>The container builder for chaining.</returns>
    public static ContainerBuilder AddGameLoopServices(this ContainerBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // Register the game loop phase registry
        builder.RegisterType<GameLoopPhaseRegistry>()
               .As<IGameLoopPhaseRegistry>()
               .SingleInstance();

        // Register the game loop factory
        builder.RegisterType<GameLoopFactory>()
               .As<IGameLoopFactory>()
               .SingleInstance();

        return builder;
    }

    /// <summary>
    /// Registers all built-in game loop phases in the DI container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <returns>The container builder for chaining.</returns>
    public static ContainerBuilder AddGameLoopPhases(this ContainerBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // Register built-in phases as transient to allow proper DI
        builder.RegisterType<NetworkUpdatePhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();

        builder.RegisterType<InjectorExecutionPhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();

        return builder;
    }

    /// <summary>
    /// Registers a custom game loop phase in the DI container.
    /// </summary>
    /// <typeparam name="TPhase">The type of the phase to register.</typeparam>
    /// <param name="builder">The container builder.</param>
    /// <returns>The container builder for chaining.</returns>
    public static ContainerBuilder AddGameLoopPhase<TPhase>(this ContainerBuilder builder)
        where TPhase : class, IGameLoopPhase
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        builder.RegisterType<TPhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();

        return builder;
    }

    /// <summary>
    /// Registers multiple custom game loop phases in the DI container.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="phaseTypes">The types of phases to register.</param>
    /// <returns>The container builder for chaining.</returns>
    public static ContainerBuilder AddGameLoopPhases(this ContainerBuilder builder, params Type[] phaseTypes)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (phaseTypes == null)
            throw new ArgumentNullException(nameof(phaseTypes));

        foreach (var phaseType in phaseTypes)
        {
            if (!typeof(IGameLoopPhase).IsAssignableFrom(phaseType))
            {
                throw new ArgumentException($"Type {phaseType.Name} must implement IGameLoopPhase", nameof(phaseTypes));
            }

            builder.RegisterType(phaseType)
                   .As<IGameLoopPhase>()
                   .InstancePerDependency();
        }

        return builder;
    }

    /// <summary>
    /// Registers all game loop services and built-in phases in one call.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <returns>The container builder for chaining.</returns>
    public static ContainerBuilder AddGameLoop(this ContainerBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder
            .AddGameLoopServices()
            .AddGameLoopPhases();
    }
}
