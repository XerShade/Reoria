using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Game.Entities.Interfaces;

namespace Reoria.Game.Entities;

/// <summary>
/// Manages entity lifecycle and ECS (Entity Component System) operations for the game.
/// </summary>
/// <remarks>
/// This class provides a high-level interface for entity management using MonoGame.Extended ECS framework.
/// It handles entity creation, destruction, and coordination with game systems.
/// The EntityManager integrates with the game loop through injector interfaces to provide
/// proper update and drawing cycles. All entity operations should go through this
/// manager to ensure proper ECS synchronization and component tracking.
/// </remarks>
public class EntityManager : IEntityManager, IDisposable, IDrawingInjector, IVariableUpdateInjector
{
    /// <summary>
    /// Gets the name of this entity manager.
    /// </summary>
    public string Name 
        => "Entity Manager";

    /// <summary>
    /// Gets the description of this entity manager's functionality.
    /// </summary>
    public string Description
        => "Provides functions for creating, updating, drawing, and destroying entities.";

    /// <summary>
    /// Gets the platform dependencies required by this entity manager.
    /// </summary>
    /// <remarks>
    /// Returns Platform.All indicating this manager works on all platforms
    /// since ECS operations are platform-agnostic.
    /// </remarks>
    public Type[] Dependencies
        => [];

    /// <summary>
    /// Gets the platform(s) that this entity manager is compatible with.
    /// </summary>
    /// <remarks>
    /// This property implements IInjector.Platform requirement.
    /// Returns Platform.All indicating this manager works on all platforms
    /// since ECS operations are platform-agnostic.
    /// </remarks>
    public Platform Platform
        => Platform.All;

    /// <summary>
    /// Gets the logger instance for this entity manager.
    /// </summary>
    protected ILogger<IEntityManager> Logger { get; init; }
    
    /// <summary>
    /// Gets the ECS world that manages entities and systems.
    /// </summary>
    /// <remarks>
    /// The World is the core ECS container that holds all entities,
    /// components, and systems. All entity operations are delegated
    /// to this world instance.
    /// </remarks>
    protected World World { get; init; }

    /// <summary>
    /// Initializes a new instance of the EntityManager class.
    /// </summary>
    /// <param name="logger">The logger instance for debugging and error reporting.</param>
    /// <param name="systems">Collection of ECS systems to be registered with the world.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or systems is null.</exception>
    /// <remarks>
    /// This constructor creates a new ECS world and registers all provided systems.
    /// The systems will be executed in the order they are provided during world updates.
    /// </remarks>
    public EntityManager(ILogger<IEntityManager> logger, IEnumerable<ISystem> systems)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        WorldBuilder builder = new();

        foreach (ISystem system in systems ?? throw new ArgumentNullException(nameof(systems)))
        {
            _ = builder.AddSystem(system);
        }

        this.World = builder.Build();
    }

    /// <summary>
    /// Releases all resources used by the entity manager.
    /// </summary>
    /// <remarks>
    /// This method disposes of the ECS world and all its entities, components,
    /// and systems. It should be called when the game is shutting down
    /// to prevent memory leaks and ensure proper cleanup.
    /// </remarks>
    public void Dispose()
    {
        this.World.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Updates all entities and systems in the ECS world.
    /// </summary>
    /// <param name="gameTime">The game time information for this update cycle.</param>
    /// <remarks>
    /// This method delegates to the underlying ECS world update method.
    /// All registered systems will be executed in their priority order.
    /// This should be called once per frame during the game loop.
    /// </remarks>
    public void Update(GameTime gameTime)
        => this.World.Update(gameTime);

    /// <summary>
    /// Called during the variable update phase of the game loop.
    /// </summary>
    /// <param name="gameTime">The game time information for this update cycle.</param>
    /// <remarks>
    /// This method implements the IVariableUpdateInjector interface to integrate
    /// with the game loop's variable update phase. Entity updates that
    /// should occur every frame should be performed here.
    /// </remarks>
    public void OnVariableUpdate(GameTime gameTime)
        => this.Update(gameTime);

    /// <summary>
    /// Renders all entities in the ECS world.
    /// </summary>
    /// <param name="gameTime">The game time information for this draw cycle.</param>
    /// <remarks>
    /// This method delegates to the underlying ECS world draw method.
    /// All entities with visible components will be rendered by their respective systems.
    /// This should be called once per frame during the drawing phase.
    /// </remarks>
    public void Draw(GameTime gameTime)
        => this.World.Draw(gameTime);

    /// <summary>
    /// Called during the drawing phase of the game loop with SpriteBatch.
    /// </summary>
    /// <param name="gameTime">The game time information for this draw cycle.</param>
    /// <param name="spriteBatch">The SpriteBatch instance for rendering operations.</param>
    /// <remarks>
    /// This method implements the IDrawingInjector interface to integrate
    /// with the game loop's drawing phase. It provides access to the
    /// SpriteBatch for 2D rendering operations.
    /// </remarks>
    public void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
        => this.Draw(gameTime);

    /// <summary>
    /// Creates a new entity in the ECS world.
    /// </summary>
    /// <returns>The newly created entity instance.</returns>
    /// <remarks>
    /// This method creates a new entity with no components.
    /// Components can be added to the entity using the entity's Set methods.
    /// The created entity will be managed by the ECS world.
    /// </remarks>
    public Entity CreateEntity()
        => this.World.CreateEntity();

    /// <summary>
    /// Destroys an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to destroy.</param>
    /// <remarks>
    /// This method removes the entity and all its components from the ECS world.
    /// The entity ID will become invalid after this operation.
    /// </remarks>
    public void DestroyEntity(int id) 
        => this.World.DestroyEntity(id);

    /// <summary>
    /// Destroys the specified entity instance.
    /// </summary>
    /// <param name="entity">The entity instance to destroy.</param>
    /// <remarks>
    /// This method removes the entity and all its components from the ECS world.
    /// This is the preferred method when you have a direct reference to the entity.
    /// </remarks>
    public void DestroyEntity(Entity entity) 
        => this.World.DestroyEntity(entity);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to retrieve.</param>
    /// <returns>The entity instance, or null if not found.</returns>
    /// <remarks>
    /// This method looks up the entity in the ECS world.
    /// Returns null if the entity doesn't exist or has been destroyed.
    /// </remarks>
    public Entity GetEntity(int entityId)
        => this.World.GetEntity(entityId);

    /// <summary>
    /// Initializes the entity manager and ECS world.
    /// </summary>
    /// <remarks>
    /// This method should be called after all systems have been registered
    /// but before the game loop starts. It initializes the ECS world
    /// and prepares all systems for entity processing.
    /// </remarks>
    public void Initialize()
        => this.World.Initialize();

    /// <summary>
    /// Gets a value indicating whether the entity manager is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, the entity manager will process updates and draw calls.
    /// When disabled, all entity operations are skipped but the world remains intact.
    /// This property delegates to the underlying ECS world state.
    /// </remarks>
    public bool IsEnabled
        => this.World.IsEnabled;

    /// <summary>
    /// Gets a value indicating whether the entity manager is visible.
    /// </summary>
    /// <remarks>
    /// When visible, the entity manager will render entities during draw calls.
    /// When not visible, all rendering operations are skipped but updates continue.
    /// This property delegates to the underlying ECS world visibility state.
    /// </remarks>
    public bool Visible
        => this.World.Visible;
}