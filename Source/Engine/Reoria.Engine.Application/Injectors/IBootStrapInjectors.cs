using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Defines an abstraction contract for an injector that can add functionality to the application bootstrapper.
/// </summary>
/// <remarks>
/// Bootstrap injectors are used during the initial application startup phase to configure
/// core services like configuration, logging, and dependency injection containers.
/// </remarks>
public interface IBootStrapInjector : IInjector
{
    // All base properties are inherited from IInjector
}

/// <summary>
/// Defines an abstraction contract for an injector that can add configuration functionality to the application bootstrapper.
/// </summary>
/// <remarks>
/// Implement this interface to add custom configuration sources or modify the configuration
/// building process during application bootstrap.
/// </remarks>
public interface IBootStrapConfigurationInjector : IBootStrapInjector
{
    /// <summary>
    /// Invoked when the bootstrapper builds its configuration.
    /// </summary>
    /// <param name="builder">The configuration builder for adding configuration sources.</param>
    /// <remarks>
    /// Use this method to add custom configuration sources, transformers, or modify
    /// the configuration building process. This is called before command-line arguments are added.
    /// </remarks>
    void OnBuildConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Defines an abstraction contract for an injector that can add logging functionality to the application bootstrapper.
/// </summary>
/// <remarks>
/// Implement this interface to configure custom logging providers, formatters, or modify
/// the logging setup during application bootstrap.
/// </remarks>
public interface IBootStrapLoggingInjector : IBootStrapInjector
{
    /// <summary>
    /// Invoked when the bootstrapper creates the logger factory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory created by the bootstrapper.</param>
    /// <param name="configuration">The configuration object being used by the bootstrapper.</param>
    /// <remarks>
    /// Use this method to add custom logging providers, configure log levels,
    /// or set up custom logging formatters. The basic Serilog setup is already configured.
    /// </remarks>
    void OnCreateLoggerFactory(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Defines an abstraction contract for an injector that can add dependency injection functionality to the application bootstrapper.
/// </summary>
/// <remarks>
/// Implement this interface to register custom services in the DI container or perform
/// post-configuration setup when the service provider is built.
/// </remarks>
public interface IBootStrapServicesInjector : IBootStrapInjector
{
    /// <summary>
    /// Invoked when the bootstrapper builds the dependency injection container.
    /// </summary>
    /// <param name="services">The container builder for registering services.</param>
    /// <remarks>
    /// Use this method to register your services with the Autofac container.
    /// Core services like configuration and logger factory are already registered.
    /// </remarks>
    void OnBuildServices(ContainerBuilder services);
    
    /// <summary>
    /// Invoked when the bootstrapper configures the dependency injection container.
    /// </summary>
    /// <param name="provider">The configured service provider.</param>
    /// <remarks>
    /// Use this method to perform post-configuration setup, resolve services
    /// needed for initialization, or configure services that depend on the built container.
    /// </remarks>
    void OnConfigureServices(IServiceProvider provider);
}

/// <summary>
/// Defines an abstraction contract for an injector that can add application classes to the application bootstrapper.
/// </summary>
/// <remarks>
/// This is a marker interface that inherits from IBootStrapServicesInjector.
/// Use this interface specifically for registering the main application class
/// and core application-level services during bootstrap.
/// </remarks>
public interface IBootStrapApplicationInjector : IBootStrapServicesInjector
{
    // Marker interface for application-specific bootstrapping functionality
    // No additional methods - inherits all functionality from IBootStrapServicesInjector
}