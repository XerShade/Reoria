using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;

namespace Reoria.Game.Entities.Interfaces;

public interface IEntityManager
{
    bool IsEnabled { get; }
    bool Visible { get; }

    Entity CreateEntity();
    void DestroyEntity(Entity entity);
    void DestroyEntity(int id);
    void Dispose();
    void Draw(GameTime gameTime);
    Entity GetEntity(int entityId);
    void Initialize();
    void Update(GameTime gameTime);
}