using MonoGame.Extended.ECS;

namespace Reoria.Game.Entities.Factories.Interfaces;

public interface IEntityFactory
{
    public Entity Create();
}