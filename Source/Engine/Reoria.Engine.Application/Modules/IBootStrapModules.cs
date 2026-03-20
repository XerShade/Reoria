using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Modules;

/// <summary>
/// Defines an abstraction contract for a module that can add functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Gets a description of what the module does.
    /// </summary>
    string Description { get; }
    /// <summary>
    /// Gets the dependencies of the module represented as a list of types.
    /// </summary>
    Type[] Dependencies { get; }
}

/// <summary>
/// Defines an abstraction contract for a module that can add configuration functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapConfigurationModule : IBootStrapModule
{
    /// <summary>
    /// Invoked when the bootstrapper gets its configuration object.
    /// </summary>
    /// <param name="builder">The <see cref="IAppConfigurationBuilder"/> that is managing the bootstrapper configuration sources.</param>
    void OnGetConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for a module that can add logging functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapLoggingModule : IBootStrapModule
{
    /// <summary>
    /// Invoked when the bootstrapper gets its logger factory object.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that was created by the bootstrapper.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> object that is being used by the bootstrapper.</param>
    void OnGetLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for a module that can add dependency injection functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapServicesModule : IBootStrapModule
{
    /// <summary>
    /// Invoked when the bootstrapper gets its dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="ContainerBuilder"/> that is being used by the bootstrapper.</param>
    void OnGetServices(ContainerBuilder services);
    /// <summary>
    /// Involked when the bootstrapper configures its dependency injection container.
    /// </summary>
    /// <param name="provider">The <see cref="IServiceProvider"/> that is being used by the bootstrapper.</param>
    void OnConfigureServices(IServiceProvider provider);
}