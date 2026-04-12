namespace Reoria.Engine.Application.Interfaces;

/// <summary>
/// Defines an abstraction contract for a class that represents an application.
/// </summary>
public interface IApplication : IDisposable
{
    /// <summary>
    /// Runs the application.
    /// </summary>
    void Run();
}