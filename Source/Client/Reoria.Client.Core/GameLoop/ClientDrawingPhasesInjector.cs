using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Extensions;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;

namespace Reoria.Client.Core.GameLoop;

/// <summary>
/// An injector that registers client-specific drawing phases in the DI container.
/// </summary>
public class ClientDrawingPhasesInjector : IApplicationServicesInjector
{
    /// <inheritdoc />
    public Type[] Dependencies => [];

    /// <inheritdoc />
    public Platform Platform => Platform.Desktop | Platform.Windows | Platform.iOS | Platform.Android;

    /// <inheritdoc />
    public string Name => "Client Drawing Phases Injector";

    /// <inheritdoc />
    public string Description => "Registers client-specific drawing phases (PreDraw, Drawing, PostDraw) to prevent server GPU crashes";

    /// <inheritdoc />
    public void OnBuildServices(ContainerBuilder services)
    {
        // Register client-specific drawing phases
        services.RegisterType<Reoria.Client.Core.GameLoop.Phases.PreDrawPhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();

        services.RegisterType<Reoria.Client.Core.GameLoop.Phases.DrawingPhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();

        services.RegisterType<Reoria.Client.Core.GameLoop.Phases.PostDrawPhase>()
               .As<IGameLoopPhase>()
               .InstancePerDependency();
    }

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    {
        // No additional configuration needed
    }
}
