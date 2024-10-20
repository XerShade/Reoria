using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Interfaces;
using System.Diagnostics;

namespace Reoria.Engine;

public abstract class EngineThread : IEngineThread
{
    protected readonly object threadLock = new();
    protected readonly ILogger<IEngineThread> logger;
    protected readonly IConfiguration configuration;
    public readonly int TicksPerSecond;
    public readonly float TickRate;
    public readonly int MaxFrameSkip;
    public readonly int TargetFrameRate;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    protected virtual void OnThreadConstructed() { }
    protected virtual bool OnThreadStartRequest() => !this.IsRunning;
    protected virtual bool OnThreadStopRequest() => this.IsRunning;
    protected virtual bool OnThreadPauseRequest() => !this.IsPaused;
    protected virtual bool OnThreadResumeRequest() => this.IsPaused;
    protected virtual void OnThreadStart() { }
    protected virtual void OnThreadDynamicTick(float deltaTime) { }
    protected virtual void OnThreadFixedTick() { }
    protected virtual void OnThreadSleep() { }
    protected virtual void OnThreadStop() { }

    public EngineThread(IServiceProvider services, int ticksPerSecond = 60)
    {
        lock (this.threadLock)
        {
            this.TicksPerSecond = ticksPerSecond;
            this.TickRate = 1000f / this.TicksPerSecond;
            this.MaxFrameSkip = 5;
            this.TargetFrameRate = 60;
            this.IsRunning = false;
            this.IsPaused = false;

            this.logger = services.GetRequiredService<ILogger<IEngineThread>>();
            this.configuration = services.GetRequiredService<IConfigurationRoot>();

            this.logger.LogInformation("Created new {Name} running on {TicksPerSecond} at {TickRate}ms.", this.GetType(), this.TicksPerSecond, this.TickRate);

            this.OnThreadConstructed();
        }
    }

    public void Start()
    {
        if (this.OnThreadStartRequest())
        {
            this.IsRunning = true;
            this.ExecuteThread();
        }
    }

    public void Stop()
    {
        lock (this.threadLock)
        {
            if (this.OnThreadStopRequest())
            {
                this.IsRunning = false;
            }
        }
    }

    public void Pause()
    {
        lock (this.threadLock)
        {
            if (this.OnThreadPauseRequest())
            {
                this.IsPaused = true;
            }
        }
    }

    public void Resume()
    {
        lock (this.threadLock)
        {
            if (this.OnThreadResumeRequest())
            {
                this.IsPaused = false;
            }
        }
    }

    protected virtual void ExecuteThread()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        float accumulator = 0f;
        float frameDuration = 1000f / this.TargetFrameRate;
        float lastFrameTime = 0;
        float deltaTime;

        this.OnThreadStart();

        while (this.IsRunning)
        {
            if (this.IsPaused)
            {
                this.logger.LogDebug("The game is currently paused, sleeping for 10 ms to lower cpu usage while idling.");
                Thread.Sleep(10);
                continue;
            }

            deltaTime = (float)stopwatch.Elapsed.TotalMilliseconds - lastFrameTime;
            lastFrameTime = (float)stopwatch.Elapsed.TotalMilliseconds;

            this.logger.LogDebug("Delta Time: {DeltaTime} ms", deltaTime);

            if (deltaTime > 1000f)
            {
                this.logger.LogWarning("Delta Time too large, capping at 1000 ms. Actual: {DeltaTime} ms", deltaTime);
                deltaTime = 1000f;
            }

            accumulator += deltaTime;

            int loops = 0;
            while (accumulator >= this.TickRate && loops < this.MaxFrameSkip)
            {
                this.OnThreadFixedTick();
                this.logger.LogDebug("Fixed tick executed, accumulator: {Accumulator}, loops: {Loops}", accumulator, loops);
                accumulator -= this.TickRate;
                loops++;
            }

            this.logger.LogDebug("Executing dynamic tick with deltaTime (in seconds): {DeltaTimeSeconds}", deltaTime / 1000f);
            this.OnThreadDynamicTick(deltaTime / 1000f);

            float sleepTime = frameDuration - deltaTime;
            if (sleepTime > 0)
            {
                this.logger.LogDebug("Sleeping for {SleepTime} ms to maintain target frame rate.", sleepTime);
                this.OnThreadSleep();
                Thread.Sleep((int)sleepTime);
            }
        }

        this.OnThreadStop();
    }
}
