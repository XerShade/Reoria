using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.Interfaces;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Engine.Application.GameLoop.Phases;

/// <summary>
/// A game loop phase that handles drawing operations for client applications.
/// </summary>
public class DrawingPhase : IGameLoopPhase
{
    private readonly ILogger<DrawingPhase> logger;
    private readonly IEnumerable<IDrawingInjector> drawingInjectors;
    private readonly SpriteBatch spriteBatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawingPhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="drawingInjectors">The collection of drawing injectors.</param>
    /// <param name="spriteBatch">The sprite batch for 2D drawing operations.</param>
    public DrawingPhase(ILogger<DrawingPhase> logger, IEnumerable<IDrawingInjector> drawingInjectors, SpriteBatch spriteBatch)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.drawingInjectors = drawingInjectors ?? throw new ArgumentNullException(nameof(drawingInjectors));
        this.spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
    }

    /// <inheritdoc />
    public string Name => "Drawing";

    /// <inheritdoc />
    public int Priority => 10; // Lowest priority, runs last

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        if (!this.drawingInjectors.Any())
        {
            this.logger.LogTrace("No drawing injectors to execute for tick {TickNumber}", context.TickNumber);
            return;
        }

        this.logger.LogTrace("Executing {InjectorCount} drawing injectors for tick {TickNumber}", 
            this.drawingInjectors.Count(), context.TickNumber);

        foreach (var injector in this.drawingInjectors)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Execute the drawing injector
                injector.OnDraw(context.GameTime, this.spriteBatch);
                
                stopwatch.Stop();

                this.logger.LogTrace("Drawing injector {InjectorType} executed in {ElapsedMilliseconds}ms", 
                    injector.GetType().Name, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error executing drawing injector {InjectorType}", injector.GetType().Name);
                throw;
            }
        }

        await Task.CompletedTask;
    }
}
