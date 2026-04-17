using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Game.Entities;

public class EntityManager : IEntityManager, IDisposable, IDrawingInjector, IVariableUpdateInjector
{
    public string Name 
        => "Entity Manager";

    public string Description
        => "Provides functions for creating, updating, drawing, and destroying entities.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All;

    protected ILogger<IEntityManager> Logger { get; init; }
    protected World World { get; init; }

    public EntityManager(ILogger<IEntityManager> logger, IEnumerable<ISystem> systems)
    {
        this.Logger = logger;

        WorldBuilder builder = new();

        foreach (ISystem system in systems)
        {
            _ = builder.AddSystem(system);
        }

        this.World = builder.Build();
    }

    public void Dispose()
    {
        this.World.Dispose();

        GC.SuppressFinalize(this);
    }

    public void Update(GameTime gameTime)
        => this.World.Update(gameTime);

    public void OnVariableUpdate(GameTime gameTime)
        => this.Update(gameTime);

    public void Draw(GameTime gameTime)
        => this.World.Draw(gameTime);

    public void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
        => this.Draw(gameTime);

    public Entity CreateEntity()
        => this.World.CreateEntity();

    public void DestroyEntity(int id) 
        => this.World.DestroyEntity(id);

    public void DestroyEntity(Entity entity) 
        => this.World.DestroyEntity(entity);

    public Entity GetEntity(int entityId)
        => this.World.GetEntity(entityId);

    public void Initialize()
        => this.World.Initialize();

    public bool IsEnabled
        => this.World.IsEnabled;

    public bool Visible
        => this.World.Visible;
}