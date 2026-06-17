using Autofac;
using Reoria.Client.Core.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Client.Application;

/// <summary>
/// Defines an inejector for injecting the client application.
/// </summary>
public class ClientApplicationInjector : IBootStrapServicesInjector
{
    /// <inheritdoc />
    public string Name
        => "Client Application Injector";

    /// <inheritdoc />
    public string Description
        => "Injects the client application into the bootstrap container.";

    /// <inheritdoc />
    public Type[] Dependencies
        => [];

    /// <inheritdoc />
    public Platform Platform
        => Platform.All & ~Platform.Server;

    /// <inheritdoc />
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ClientApplication>()
        .Keyed<IApplication>("ClientApplication")
        .As<ClientApplication>()
        .As<IApplication>()
        .SingleInstance();

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    { }
}