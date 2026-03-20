using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Modules;

/// <summary>
/// Defines an abstraction contract for a module that can add functionality to the application.
/// </summary>
public interface IApplicationModule
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
/// Defines an abstraction contract for a module that can add configuration functionality to the application.
/// </summary>
public interface IApplicationConfigurationModule : IApplicationModule
{
    /// <summary>
    /// Invoked when the application gets its configuration object.
    /// </summary>
    /// <param name="builder">The <see cref="IAppConfigurationBuilder"/> that is managing the application configuration sources.</param>
    void OnGetConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for a module that can add logging functionality to the application.
/// </summary>
public interface IApplicationLoggingModule : IApplicationModule
{
    /// <summary>
    /// Invoked when the application gets its logger factory object.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that was created by the application.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> object that is being used by the application.</param>
    void OnGetLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for a module that can add dependency injection functionality to the application.
/// </summary>
public interface IApplicationServicesModule : IApplicationModule
{
    /// <summary>
    /// Invoked when the application gets its dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="ContainerBuilder"/> that is being used by the application.</param>
    void OnGetServices(ContainerBuilder services);
    /// <summary>
    /// Involked when the application configures its dependency injection container.
    /// </summary>
    /// <param name="provider">The <see cref="IServiceProvider"/> that is being used by the application.</param>
    void OnConfigureServices(IServiceProvider provider);
}