using Microsoft.Xna.Framework;

namespace Reoria.Engine.Application.Threads;

/// <summary>
/// Defines an abstraction contract for a class that can run a thread for execution of logic.
/// </summary>
public interface IGameThread
{
    /// <summary>
    /// Gets the thread's game service container.
    /// </summary>
    GameServiceContainer Services { get; }

    /// <summary>
    /// Runs the thread and executes logic.
    /// </summary>
    void Run();
}