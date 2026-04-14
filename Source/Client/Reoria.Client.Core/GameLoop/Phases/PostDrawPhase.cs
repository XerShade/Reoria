using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Client.Core.GameLoop.Phases;

/// <summary>
/// A game loop phase that finalizes rendering pipeline after drawing operations.
/// </summary>
public class PostDrawPhase : IGameLoopPhase
{
    private readonly ILogger<PostDrawPhase> logger;
    private readonly SpriteBatch spriteBatch;
    private readonly GraphicsDevice graphicsDevice;
    private readonly IEnumerable<IPostDrawInjector> postDrawInjectors;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostDrawPhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="spriteBatch">The sprite batch.</param>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="postDrawInjectors">The collection of post-draw injectors.</param>
    public PostDrawPhase(
        ILogger<PostDrawPhase> logger, 
        SpriteBatch spriteBatch, 
        GraphicsDevice graphicsDevice, 
        IEnumerable<IPostDrawInjector> postDrawInjectors)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        this.postDrawInjectors = postDrawInjectors ?? throw new ArgumentNullException(nameof(postDrawInjectors));
    }

    /// <inheritdoc />
    public string Name => "Post-Draw";

    /// <inheritdoc />
    public int Priority => 5; // Runs after DrawingPhase (lowest priority)

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsAsync => false; // Synchronous only

    /// <inheritdoc />
    public Task Execute(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        this.logger.LogTrace("Finalizing rendering pipeline for tick {TickNumber}", context.TickNumber);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // End the sprite batch to submit all drawing operations
            this.spriteBatch.End();

            // Execute post-draw injectors
            if (this.postDrawInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} post-draw injectors for tick {TickNumber}", 
                    this.postDrawInjectors.Count(), context.TickNumber);

                foreach (var injector in this.postDrawInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var injectorStopwatch = System.Diagnostics.Stopwatch.StartNew();
                        
                        injector.OnPostDraw(context.GameTime, this.graphicsDevice);
                        
                        injectorStopwatch.Stop();

                        this.logger.LogTrace("Post-draw injector {InjectorType} executed in {ElapsedMilliseconds}ms", 
                            injector.GetType().Name, injectorStopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing post-draw injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }

            stopwatch.Stop();
            this.logger.LogTrace("Post-draw cleanup completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during post-draw cleanup for tick {TickNumber}", context.TickNumber);
            throw;
        }

        return Task.CompletedTask;
    }
}
