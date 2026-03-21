using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Reoria.Engine.Application.Threads;
using System.Diagnostics;

namespace Reoria.Server.Threads;

public class GameThread(ILogger<IGameThread> logger) : IGameThread
{
    protected virtual ILogger<IGameThread> Logger { get; init; } = logger;
    protected virtual Stopwatch Timer { get; init; } = new();
    protected virtual TimeSpan PreviousTime { get; set; }
    protected virtual TimeSpan Accumulator { get; set; }
    protected virtual TimeSpan FixedStep { get; init; } = TimeSpan.FromSeconds(1.0 / 30.0);
    protected virtual bool Running { get; set; } = true;
    protected virtual int MaxSteps { get; init; } = 5;
    protected virtual int Steps { get; set; } = 0;

    public virtual GameServiceContainer Services
        => throw new NotImplementedException("The server does not have a game service container.");

    public virtual void Run()
    {
        this.Timer.Start();
        this.PreviousTime = this.Timer.Elapsed;

        while (this.Running)
        {
            TimeSpan now = this.Timer.Elapsed;
            TimeSpan frameTime = now - this.PreviousTime;
            this.PreviousTime = now;

            this.Accumulator += frameTime;

            GameTime variableGameTime = new(now, frameTime);
            this.VariableUpdate(variableGameTime);

            while (this.Accumulator >= this.FixedStep && this.Steps < this.MaxSteps)
            {
                GameTime fixedGameTime = new(now, this.FixedStep);
                this.FixedUpdate(fixedGameTime);

                this.Accumulator -= this.FixedStep;
                this.Steps++;
            }

            this.Steps = 0;

            Thread.Sleep(1);
        }
    }

    protected virtual void VariableUpdate(GameTime gameTime)
    {

    }

    protected virtual void FixedUpdate(GameTime gameTime)
    {

    }

    public virtual void Exit()
        => this.Running = false;
}