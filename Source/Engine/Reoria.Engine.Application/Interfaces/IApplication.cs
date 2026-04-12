using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Injectors;

namespace Reoria.Engine.Application.Interfaces;

/// <summary>
/// Defines an abstraction contract for a class that represents an application.
/// </summary>
public interface IApplication : IDisposable
{
    /// <summary>
    /// Gets an instance of <see cref="ILogger"/> that can be used to log messages.
    /// </summary>
    ILogger<IApplication> Logger { get; }
    /// <summary>
    /// Gets an instance of a collection of <see cref="IApplicationInjector"/> that can be used to inject functionality.
    /// </summary>
    List<IApplicationInjector> Injectors { get; }
    /// <summary>
    /// Gets an instance of <see cref="IConfiguration"/> that can be used to access configuration settings.
    /// </summary>
    IConfiguration Configuration { get; }
    /// <summary>
    /// Gets an instance of <see cref="ILoggerFactory"/> that can be used to create loggers.
    /// </summary>
    ILoggerFactory LoggerFactory { get; }
    /// <summary>
    /// Gets an instance of <see cref="ContainerBuilder"/> that can be used to register services.
    /// </summary>
    ContainerBuilder ContainerBuilder { get; }
    /// <summary>
    /// Gets an instance of <see cref="IServiceProvider"/> that can be used to resolve services.
    /// </summary>
    IServiceProvider Provider { get; }

    /// <summary>
    /// Runs the application.
    /// </summary>
    void Run();
    /// <summary>
    /// Exit the application at the end of the current tick.
    /// </summary>
    void Exit();
}