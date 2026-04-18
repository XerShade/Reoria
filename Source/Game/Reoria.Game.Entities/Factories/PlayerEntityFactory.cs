using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using Reoria.Engine.Core.Components;
using Reoria.Game.Entities.Factories.Interfaces;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Engine.Core.Factories;

public class PlayerEntityFactory(IEntityManager manager) : IEntityFactory
{
    protected virtual IEntityManager Manager { get; set; } = manager;
    protected virtual Random Random { get; set; } = new Random();

    public Entity Create()
    {
        Entity entity = this.Manager.CreateEntity();

        entity.Attach(new TransformComponent()
        {
            Position = new Vector2(this.Random.Next(0, 100), this.Random.Next(0, 100))
        });

        entity.Attach(new NetworkInformationComponent()
        {

        });

        return entity;
    }
}