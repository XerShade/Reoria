using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Defines an abstraction contract for a injector that can add functionality to the application.
/// </summary>
public interface IApplicationInjector
{
    /// <summary>
    /// Gets the name of the injector.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Gets a description of what the injector does.
    /// </summary>
    string Description { get; }
    /// <summary>
    /// Gets the dependencies of the injector represented as a list of types.
    /// </summary>
    Type[] Dependencies { get; }
}

/// <summary>
/// Defines an abstraction contract for a injector that can add configuration functionality to the application.
/// </summary>
public interface IApplicationConfigurationInjector : IApplicationInjector
{
    /// <summary>
    /// Invoked when the application gets its configuration object.
    /// </summary>
    /// <param name="builder">The <see cref="IAppConfigurationBuilder"/> that is managing the application configuration sources.</param>
    void OnGetConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for a injector that can add logging functionality to the application.
/// </summary>
public interface IApplicationLoggingInjector : IApplicationInjector
{
    /// <summary>
    /// Invoked when the application gets its logger factory object.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that was created by the application.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> object that is being used by the application.</param>
    void OnGetLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for a injector that can add dependency injection functionality to the application.
/// </summary>
public interface IApplicationServicesInjector : IApplicationInjector
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