using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Reoria.Engine.Application.Threads;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Reoria.Client.Core.Threads;

public class GameThread : Game, IGameThread
{
    private GraphicsDeviceManager GraphicsDeviceManager { get; set; }
    private SpriteBatch? SpriteBatch { get; set; }
    protected TimeSpan Accumulator { get; set; }
    protected TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 30.0);
    protected int MaxSteps { get; init; } = 5;
    protected int Steps { get; set; } = 0;

    public GameThread()
    {
        this.GraphicsDeviceManager = new GraphicsDeviceManager(this);
        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    protected override void LoadContent()
        => this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

    protected override void Update(GameTime gameTime)
    {
#if !IOS
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            this.Exit();
        }
#endif

        this.Accumulator += gameTime.ElapsedGameTime;

        this.VariableUpdate(gameTime);

        while (this.Accumulator >= this.FixedStep && this.Steps < this.MaxSteps)
        {
            GameTime fixedGameTime = new(gameTime.TotalGameTime, this.FixedStep);
            this.FixedUpdate(fixedGameTime);

            this.Accumulator -= this.FixedStep;
            this.Steps++;
        }

        this.Steps = 0;

        base.Update(gameTime);
    }

    protected virtual void VariableUpdate(GameTime gameTime)
    {

    }

    protected virtual void FixedUpdate(GameTime gameTime)
    {

    }

    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
