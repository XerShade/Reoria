using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Core.Components;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace Reoria.Client.Entities.Systems;

public class EntityRenderSystem(ContentManager content, SpriteBatch spriteBatch)
    : EntityDrawSystem(Aspect.All(typeof(TransformComponent)))
{
    protected virtual ContentManager Content { get; init; } = content;
    protected virtual SpriteBatch SpriteBatch { get; init; } = spriteBatch;
    protected virtual ComponentMapper<TransformComponent>? TransformComponentMapper { get; set; }
    protected virtual Texture2D? Texture { get; set; }

    public override void Initialize(IComponentMapperService mapperService)
    {
        this.TransformComponentMapper = mapperService.GetMapper<TransformComponent>();
        this.Texture = this.Content.Load<Texture2D>("Graphics/Characters/TimeFantasy/chara1");
    }

    public override void Draw(GameTime gameTime)
    {
        if(this.Texture is null)
        {
            return;
        }

        if (this.TransformComponentMapper is null)
        {
            return;
        }

        Rectangle sourceRectangle = new(0, 0, this.Texture.Width / 4 / 3, this.Texture.Height / 2 / 4);

        foreach(int entity in this.ActiveEntities)
        {
            TransformComponent transformComponent = this.TransformComponentMapper.Get(entity);
            Rectangle destinationRectangle = new((int)transformComponent.Position.X, (int)transformComponent.Position.Y, sourceRectangle.Width, sourceRectangle.Height);

            this.SpriteBatch.Draw(this.Texture, destinationRectangle, sourceRectangle, Color.White);
        }
    }
}