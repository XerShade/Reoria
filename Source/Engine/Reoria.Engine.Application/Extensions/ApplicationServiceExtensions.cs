using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Engine.Application.Extensions;

/// <summary>
/// Defines extension methods for adding dependency injection functionality to an application.
/// </summary>
/// <remarks>
/// These extensions provide dependency injection setup capabilities for applications,
/// allowing injectors to participate in the service registration and configuration process.
/// </remarks>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Gets the Autofac container builder for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="ContainerBuilder"/>.</returns>
    /// <remarks>
    /// This method creates and configures the Autofac container builder by:
    /// 1. Creating a new container builder
    /// 2. Registering core services (configuration, logger factory)
    /// 3. Invoking application service injectors to register custom services
    /// 4. Returning the configured builder for further customization
    /// </remarks>
    public static ContainerBuilder GetServices(this IApplication application)
    {
        // Create a new Autofac container builder.
        ContainerBuilder services = new();

        // Register core application services as singletons.
        _ = services.RegisterInstance(application.Configuration)
            .As<IConfiguration>()
            .SingleInstance();
        _ = services.RegisterInstance(application.LoggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();
        _ = services.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        // Invoke application service injectors to register custom services.
        foreach (IApplicationServicesInjector injector in application.Injectors.OfType<IApplicationServicesInjector>())
        {
            injector.OnBuildServices(services);
        }

        // Return the configured container builder.
        return services;
    }

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="IServiceProvider"/>.</returns>
    /// <remarks>
    /// This method builds the dependency injection container and configures it by:
    /// 1. Building the Autofac container from the container builder
    /// 2. Creating an Autofac service provider
    /// 3. Invoking application service injectors for post-configuration
    /// 4. Returning the configured service provider
    /// </remarks>
    public static IServiceProvider GetServiceProvider(this IApplication application)
    {
        // Build the Autofac container from the configured container builder.
        IContainer container = application.ContainerBuilder.Build();

        // Create an Autofac service provider from the built container.
        AutofacServiceProvider provider = new(container);

        // Invoke application service injectors for post-configuration setup.
        foreach (IApplicationServicesInjector injector in application.Injectors.OfType<IApplicationServicesInjector>())
        {
            injector.OnConfigureServices(provider);
        }

        // Return the configured service provider.
        return provider;
    }
}