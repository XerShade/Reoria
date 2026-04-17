using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Game.Entities;

public class EntityManager : IEntityManager, IDisposable
{
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

    public void Draw(GameTime gameTime) 
        => this.World.Draw(gameTime);

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