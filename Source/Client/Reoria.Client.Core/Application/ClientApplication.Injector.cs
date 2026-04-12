using Autofac;
using Reoria.Client.Core.Application;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Client.Application;

/// <summary>
/// Defines an inejector for injecting the client application.
/// </summary>
public class ClientApplicationInjector : IBootStrapApplicationInjector
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
    public void OnGetServices(ContainerBuilder services) 
        => services.RegisterType<ClientApplication>()
        .Keyed<IApplication>("ClientApplication")
        .As<ClientApplication>()
        .As<IApplication>()
        .SingleInstance();

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider) 
    { }
}