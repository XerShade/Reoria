using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Core.Components;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace Reoria.Client.Entities.Systems;

/// <summary>
/// System for rendering entities with TransformComponent using sprite batch rendering.
/// </summary>
/// <remarks>
/// This system handles the visual representation of entities in the game world.
/// It renders entities as textured sprites positioned according to their TransformComponent.
/// The system uses MonoGame.Extended ECS framework to efficiently query for
/// entities that have both TransformComponent and processes them in batch.
/// Only entities with TransformComponent will be processed by this system.
/// </remarks>
public class EntityRenderSystem(ContentManager content, SpriteBatch spriteBatch)
    : EntityDrawSystem(Aspect.All(typeof(TransformComponent)))
{
    /// <summary>
    /// Gets the content manager for loading texture assets.
    /// </summary>
    /// <remarks>
    /// The content manager is used to load sprite textures.
    /// This property is initialized during system initialization and used
    /// to load the character sprite texture for rendering entities.
    /// </remarks>
    protected virtual ContentManager Content { get; init; } = content;
    
    /// <summary>
    /// Gets the sprite batch for rendering operations.
    /// </summary>
    /// <remarks>
    /// The sprite batch is used for efficient 2D rendering operations.
    /// This property is initialized during system initialization and used
    /// for all drawing operations in the Draw method.
    /// </remarks>
    protected virtual SpriteBatch SpriteBatch { get; init; } = spriteBatch;
    
    /// <summary>
    /// Gets the component mapper for TransformComponent instances.
    /// </summary>
    /// <remarks>
    /// The mapper provides efficient access to TransformComponent data
    /// for entities being processed by this render system.
    /// This property is initialized during system initialization.
    /// </remarks>
    protected virtual ComponentMapper<TransformComponent>? TransformComponentMapper { get; set; }
    
    /// <summary>
    /// Gets or sets the texture used for rendering entities.
    /// </summary>
    /// <remarks>
    /// The texture is loaded from content and represents the visual
    /// appearance of entities. This property can be set to null to
    /// disable rendering or change entity appearance.
    /// </remarks>
    protected virtual Texture2D? Texture { get; set; }

    /// <summary>
    /// Initializes the entity render system.
    /// </summary>
    /// <param name="mapperService">The component mapper service for ECS integration.</param>
    /// <remarks>
    /// This method sets up the component mapper and loads the default
    /// texture for entity rendering. The texture is loaded from the
    /// Graphics/Characters/TimeFantasy/chara1 path and represents the default
    /// character sprite for all entities.
    /// </remarks>
    public override void Initialize(IComponentMapperService mapperService)
    {
        this.TransformComponentMapper = mapperService.GetMapper<TransformComponent>();
        this.Texture = this.Content.Load<Texture2D>("Graphics/Characters/TimeFantasy/chara1");
    }

    /// <summary>
    /// Renders all entities with TransformComponent using sprite batch.
    /// </summary>
    /// <param name="gameTime">The game time information for this frame.</param>
    /// <remarks>
    /// This method is called by the game loop during the drawing phase.
    /// It iterates through all entities that have TransformComponent
    /// and renders them as sprites at their world positions.
    /// The source rectangle is calculated as 1/4 of the texture size
    /// to render only the top-left quadrant of the sprite.
    /// Entities are rendered with white color tint to preserve original texture colors.
    /// </remarks>
    public override void Draw(GameTime gameTime)
    {
        // Early return if texture is not loaded
        if(this.Texture is null)
        {
            return;
        }

        // Early return if component mapper is not available
        if (this.TransformComponentMapper is null)
        {
            return;
        }

        // Calculate source rectangle for sprite rendering (top-left quadrant)
        Rectangle sourceRectangle = new(0, 0, this.Texture.Width / 4 / 3, this.Texture.Height / 2 / 4);

        // Render each entity with TransformComponent
        foreach(int entity in this.ActiveEntities)
        {
            TransformComponent transformComponent = this.TransformComponentMapper.Get(entity);
            Rectangle destinationRectangle = new((int)transformComponent.Position.X, (int)transformComponent.Position.Y, sourceRectangle.Width, sourceRectangle.Height);

            this.SpriteBatch.Draw(this.Texture, destinationRectangle, sourceRectangle, Color.White);
        }
    }
}