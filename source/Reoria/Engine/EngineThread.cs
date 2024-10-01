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

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    protected virtual void OnThreadConstructed() { }
    protected virtual bool OnThreadStartRequest() => !this.IsRunning;
    protected virtual bool OnThreadStopRequest() => this.IsRunning;
    protected virtual bool OnThreadPauseRequest() => !this.IsPaused;
    protected virtual bool OnThreadResumeRequest() => this.IsPaused;
    protected virtual void OnThreadStart() { }
    protected virtual void OnThreadTick() { }
    protected virtual void OnThreadSleep() { }
    protected virtual void OnThreadStop() { }

    public EngineThread(IServiceProvider services, int ticksPerSecond = 60)
    {
        lock (this.threadLock)
        {
            this.TicksPerSecond = ticksPerSecond;
            this.TickRate = 1000f / this.TicksPerSecond;
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

        this.OnThreadStart();

        while (this.IsRunning)
        {
            if (this.IsPaused)
            {
                Thread.Sleep(10);
                continue;
            }

            stopwatch.Start();

            this.OnThreadTick();

            stopwatch.Stop();

            int elapsedMs = (int)stopwatch.ElapsedMilliseconds;
            if (elapsedMs < this.TickRate)
            {
                this.OnThreadSleep();
                Thread.Sleep((int)this.TickRate - elapsedMs);
            }

            stopwatch.Reset();
        }

        this.OnThreadStop();
    }
}
