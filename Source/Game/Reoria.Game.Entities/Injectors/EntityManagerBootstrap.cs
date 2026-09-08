using Autofac;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;
using Reoria.Engine.Core.Components.Interfaces;
using Reoria.Engine.Core.Reflection;
using Reoria.Game.Entities.Factories.Interfaces;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Game.Entities.Injectors;

/// <summary>
/// Bootstrap services phase participant for entity component system functionality.
/// Discovers and registers entity systems, factories, and components during bootstrap.
/// </summary>
/// <remarks>
/// Runs during bootstrap phase - no constructor dependencies allowed.
/// </remarks>
public class EntityManagerBootstrap : IBootstrapServices
{
    public string Name
        => "Entity Manager Bootstrap";

    public string Description
        => "Discovers and registers entity component systems, factories, and components during bootstrap.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All;

    public void OnRegisterServices(ContainerBuilder services)
    {
        _ = services.RegisterType<EntityManager>()
            .As<IEntityManager>().AsImplementedInterfaces()
            .SingleInstance();

        this.DiscoverSystems(services);
        this.DiscoverFactories(services);
        this.DiscoverComponents(services);
    }

    protected virtual void DiscoverSystems(ContainerBuilder services)
    {
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<ISystem>();

        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<ISystem>().AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }

    protected virtual void DiscoverFactories(ContainerBuilder services)
    {
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<IEntityFactory>();

        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IEntityFactory>().AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
    protected virtual void DiscoverComponents(ContainerBuilder services)
    {
        Type[] types = TypeDiscoveryHelper.GetConcreteTypesImplementingInterface<IComponent>();

        foreach (Type type in types)
        {
            _ = services.RegisterType(type)
                .As(type).As<IComponent>().AsImplementedInterfaces()
                .InstancePerDependency();
        }
    }
}