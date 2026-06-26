using Autofac;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Injectors;
using GameBase = Microsoft.Xna.Framework.Game;

namespace Reoria.Client.Core.Injectors;

public interface ILifeCycleInitializeGraphicsInjector : IInjector
{
    void OnInitializeGraphics(ContainerBuilder services, GraphicsDeviceManager graphicsDeviceManager, GraphicsDevice graphicsDevice);
}

public interface ILifeCycleInitializeGameInjector : IInjector
{
    void OnInitializeGame(GameBase game);
}

public interface ILifeCycleLoadContentInjector : IInjector
{
    void OnLoadContent(ContainerBuilder services, ContentManager contentManager);
}

public interface ILifeCycleFinalizeInjector : IInjector
{
    void OnFinalize(ContainerBuilder services);
}