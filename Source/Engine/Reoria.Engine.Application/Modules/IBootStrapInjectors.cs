using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Defines an abstraction contract for a injector that can add functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapInjector
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
/// Defines an abstraction contract for a injector that can add configuration functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapConfigurationInjector : IBootStrapInjector
{
    /// <summary>
    /// Invoked when the bootstrapper gets its configuration object.
    /// </summary>
    /// <param name="builder">The <see cref="IAppConfigurationBuilder"/> that is managing the bootstrapper configuration sources.</param>
    void OnGetConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for a injector that can add logging functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapLoggingInjector : IBootStrapInjector
{
    /// <summary>
    /// Invoked when the bootstrapper gets its logger factory object.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that was created by the bootstrapper.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> object that is being used by the bootstrapper.</param>
    void OnGetLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for a injector that can add dependency injection functionality to the application bootstrapper.
/// </summary>
public interface IBootStrapServicesInjector : IBootStrapInjector
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

/// <summary>
/// Defines an abstraction contract for a injector that can add application classes application bootstrapper.
/// </summary>
public interface IBootStrapApplicationInjector : IBootStrapServicesInjector
{
    // Note: This is a marker interface, for now it is just an empty interface that inherits from IBootStrapServicesInjector.
}