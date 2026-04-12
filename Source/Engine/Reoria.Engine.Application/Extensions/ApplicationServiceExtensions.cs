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
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Gets the Autofac container builder for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="ContainerBuilder"/>.</returns>
    public static ContainerBuilder GetServices(this IApplication application)
    {
        // Construct the container builder.
        ContainerBuilder services = new();

        // Register the configuration and logger factories.
        _ = services.RegisterInstance(application.Configuration)
            .As<IConfiguration>()
            .SingleInstance();
        _ = services.RegisterInstance(application.LoggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();
        _ = services.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        // Iterate over the injectors.
        foreach (IApplicationServicesInjector injector in application.Injectors.OfType<IApplicationServicesInjector>())
        {
            // Invoke the injector.
            injector.OnGetServices(services);
        }

        // Return the container builder.
        return services;
    }

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    /// <param name="application">The <see cref="IApplication"/> instance being extended.</param>
    /// <returns>An instance of <see cref="IServiceProvider"/>.</returns>
    public static IServiceProvider GetServiceProvider(this IApplication application)
    {
        // Build the container.
        IContainer container = application.ContainerBuilder.Build();

        // Create the service provider.
        AutofacServiceProvider provider = new(container);

        // Iterate over the injectors.
        foreach (IApplicationServicesInjector injector in application.Injectors.OfType<IApplicationServicesInjector>())
        {
            // Invoke the injector.
            injector.OnConfigureServices(provider);
        }
        
        // Return the provider.
        return provider;
    }
}