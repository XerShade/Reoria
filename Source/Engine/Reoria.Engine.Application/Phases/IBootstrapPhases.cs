using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;
using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application.Phases;

/// <summary>
/// Base interface for all bootstrap phase participants.
/// Bootstrap phase runs before the DI container is built - no DI dependencies allowed.
/// </summary>
public interface IBootstrapPhase : IPhaseParticipant
{
    // All properties inherited from IPhaseParticipant
}

/// <summary>
/// Bootstrap phase for adding configuration sources.
/// Runs before DI container is built - no constructor dependencies allowed.
/// </summary>
public interface IBootstrapConfiguration : IBootstrapPhase
{
    /// <summary>
    /// Called during bootstrap to add configuration sources.
    /// </summary>
    /// <param name="builder">The configuration builder to add sources to.</param>
    void OnBuildConfiguration(IAppConfigurationBuilder builder);
}

/// <summary>
/// Bootstrap phase for configuring logging.
/// Runs before DI container is built - no constructor dependencies allowed.
/// </summary>
public interface IBootstrapLogging : IBootstrapPhase
{
    /// <summary>
    /// Called during bootstrap to configure logging.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to configure.</param>
    /// <param name="configuration">The configuration object.</param>
    void OnConfigureLogging(ILoggerFactory loggerFactory, IConfiguration configuration);
}

/// <summary>
/// Bootstrap phase for registering services in the DI container.
/// Runs before DI container is built - no constructor dependencies allowed.
/// This is where infrastructure services are registered.
/// </summary>
public interface IBootstrapServices : IBootstrapPhase
{
    /// <summary>
    /// Called during bootstrap to register services in the DI container.
    /// No constructor dependencies allowed - use ContainerBuilder parameters only.
    /// </summary>
    /// <param name="services">The container builder to register services with.</param>
    void OnRegisterServices(ContainerBuilder services);
}

/// <summary>
/// Bootstrap phase for post-configuration after DI container is built.
/// Runs after DI container is built - can resolve services from the container.
/// </summary>
public interface IBootstrapPostConfigure : IBootstrapPhase
{
    /// <summary>
    /// Called after the DI container is built for post-configuration.
    /// Can resolve services from the DI container via the provider parameter.
    /// </summary>
    /// <param name="provider">The built service provider.</param>
    void OnPostConfigure(IServiceProvider provider);
}
