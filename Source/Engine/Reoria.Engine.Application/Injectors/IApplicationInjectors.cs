using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Defines an abstraction contract for an injector that can add functionality to the application.
/// </summary>
/// <remarks>
/// Application injectors are used after the bootstrap phase to configure application-specific
/// services, handle application lifecycle events, and provide runtime functionality.
/// </remarks>
public interface IApplicationInjector : IInjector
{
    // All base properties are inherited from IInjector
}

/// <summary>
/// Defines an abstraction contract for an injector that can participate in application lifecycle events.
/// </summary>
/// <remarks>
/// Implement this interface to handle application startup and shutdown events.
/// This is useful for initializing resources, starting background services,
/// or performing cleanup when the application terminates.
/// </remarks>
public interface IApplicationLifecycleInjector : IApplicationInjector
{
    /// <summary>
    /// Called when the application starts.
    /// </summary>
    /// <remarks>
    /// This method is called after all services have been registered and the application
    /// is ready to start. Use this for initialization tasks that require the full DI container.
    /// </remarks>
    void OnApplicationStart();

    /// <summary>
    /// Called when the application stops.
    /// </summary>
    /// <remarks>
    /// This method is called during graceful shutdown. Use this for cleanup tasks,
    /// disposing resources, or stopping background services.
    /// </remarks>
    void OnApplicationStop();
}

/// <summary>
/// Defines an abstraction contract for an injector that can add configuration functionality to the application.
/// </summary>
/// <remarks>
/// Implement this interface to add application-specific configuration sources or modify
/// the configuration building process at the application level (not bootstrap level).
/// </remarks>
public interface IApplicationConfigurationInjector : IApplicationInjector
{
    /// <summary>
    /// Invoked when the application builds its configuration.
    /// </summary>
    /// <param name="builder">The configuration builder for adding configuration sources.</param>
    /// <remarks>
    /// Use this method to add application-specific configuration sources that are
    /// not needed during the bootstrap phase but are required for the running application.
    /// </remarks>
    void OnBuildConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for an injector that can add logging functionality to the application.
/// </summary>
/// <remarks>
/// Implement this interface to configure application-specific logging providers or modify
/// the logging setup at the application level (not bootstrap level).
/// </remarks>
public interface IApplicationLoggingInjector : IApplicationInjector
{
    /// <summary>
    /// Invoked when the application creates the logger factory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory created by the application.</param>
    /// <param name="configuration">The configuration object being used by the application.</param>
    /// <remarks>
    /// Use this method to add application-specific logging providers or configure
    /// logging that is not needed during the bootstrap phase.
    /// </remarks>
    void OnCreateLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for an injector that can add dependency injection functionality to the application.
/// </summary>
/// <remarks>
/// Implement this interface to register application-specific services in the DI container or perform
/// post-configuration setup at the application level (not bootstrap level).
/// </remarks>
public interface IApplicationServicesInjector : IApplicationInjector
{
    /// <summary>
    /// Invoked when the application builds the dependency injection container.
    /// </summary>
    /// <param name="services">The container builder for registering services.</param>
    /// <remarks>
    /// Use this method to register application-specific services that are not needed
    /// during the bootstrap phase but are required for the running application.
    /// </remarks>
    void OnBuildServices(ContainerBuilder services);
    
    /// <summary>
    /// Invoked when the application configures the dependency injection container.
    /// </summary>
    /// <param name="provider">The configured service provider.</param>
    /// <remarks>
    /// Use this method to perform application-level post-configuration setup or resolve
    /// services needed for application initialization.
    /// </remarks>
    void OnConfigureServices(IServiceProvider provider);
}
