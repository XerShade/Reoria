using Autofac;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Core.Reflection;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Game.Entities.Injectors;

public class EntityManagerInjector : IApplicationServicesInjector
{
    public string Name 
        => "Entity Manager Injector";

    public string Description 
        => "Injects entity component systems functionality into the application.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All;

    public void OnBuildServices(ContainerBuilder services)
    {
        _ = services.RegisterType<EntityManager>()
            .As<IEntityManager>()
            .SingleInstance();

        this.DiscoverSystems(services);
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

    public void OnConfigureServices(IServiceProvider provider)
    {
        // No additional configuration is required.
    }
}