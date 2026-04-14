using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Reoria.Engine.Application.GameLoop.Interfaces;
using Reoria.Engine.Application.GameLoop.Phases.Interfaces;
using Reoria.Engine.Application.Injectors;
using Color = Microsoft.Xna.Framework.Color;

namespace Reoria.Engine.Application.GameLoop.Phases;

/// <summary>
/// A game loop phase that prepares the rendering pipeline for drawing operations.
/// </summary>
public class PreDrawPhase : IGameLoopPhase
{
    private readonly ILogger<PreDrawPhase> logger;
    private readonly GraphicsDevice graphicsDevice;
    private readonly SpriteBatch spriteBatch;
    private readonly IEnumerable<IPreDrawInjector> preDrawInjectors;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreDrawPhase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="graphicsDevice">The graphics device for rendering operations.</param>
    /// <param name="spriteBatch">The sprite batch for 2D drawing operations.</param>
    /// <param name="preDrawInjectors">The collection of pre-draw injectors.</param>
    public PreDrawPhase(ILogger<PreDrawPhase> logger, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, IEnumerable<IPreDrawInjector> preDrawInjectors)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        this.spriteBatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
        this.preDrawInjectors = preDrawInjectors ?? throw new ArgumentNullException(nameof(preDrawInjectors));
    }

    /// <inheritdoc />
    public string Name => "Pre-Draw";

    /// <inheritdoc />
    public int Priority => 15; // Runs after InjectorExecutionPhase (90) but before DrawingPhase (10)

    /// <inheritdoc />
    public bool IsEnabled => true;

    /// <inheritdoc />
    public bool IsAsync => false; // Synchronous only

    /// <inheritdoc />
    public Task Execute(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        this.logger.LogTrace("Setting up rendering pipeline for tick {TickNumber}", context.TickNumber);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Set up the graphics device for drawing
            this.graphicsDevice.BlendState = BlendState.AlphaBlend;
            this.graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            this.graphicsDevice.Clear(Color.Cyan);

            // Begin the sprite batch
            this.spriteBatch.Begin();

            // Execute pre-draw injectors
            if (this.preDrawInjectors.Any())
            {
                this.logger.LogTrace("Executing {InjectorCount} pre-draw injectors for tick {TickNumber}",
                    this.preDrawInjectors.Count(), context.TickNumber);

                foreach (var injector in this.preDrawInjectors)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var injectorStopwatch = System.Diagnostics.Stopwatch.StartNew();

                        injector.OnPreDraw(context.GameTime, this.graphicsDevice);

                        injectorStopwatch.Stop();

                        this.logger.LogTrace("Post-draw injector {InjectorType} executed in {ElapsedMilliseconds}ms",
                            injector.GetType().Name, injectorStopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error executing pre-draw injector {InjectorType}", injector.GetType().Name);
                        throw;
                    }
                }
            }

            stopwatch.Stop();
            this.logger.LogTrace("Pre-draw setup completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during pre-draw setup for tick {TickNumber}", context.TickNumber);
            throw;
        }

        return Task.CompletedTask;
    }
}
