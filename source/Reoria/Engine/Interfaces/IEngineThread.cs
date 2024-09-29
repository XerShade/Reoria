namespace Reoria.Engine.Interfaces;

public interface IEngineThread
{
    bool IsPaused { get; }
    bool IsRunning { get; }

    void Pause();
    void Resume();
    void Start();
    void Stop();
}