using Autofac;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Injectors;
using Reoria.Engine.Application.Interfaces;

namespace Reoria.Server.Core.Application;

/// <summary>
/// Defines an inejector for injecting the server application.
/// </summary>
public class ServerApplicationInjector : IBootStrapServicesInjector
{
    /// <inheritdoc />
    public string Name
        => "Server Application Injector";

    /// <inheritdoc />
    public string Description
        => "Injects the server application into the bootstrap container.";

    /// <inheritdoc />
    public Type[] Dependencies
        => [];

    /// <inheritdoc />
    public Platform Platform
        => Platform.Server;

    /// <inheritdoc />
    public void OnBuildServices(ContainerBuilder services)
        => services.RegisterType<ServerApplication>()
        .Keyed<IApplication>("ServerApplication")
        .As<ServerApplication>()
        .As<IApplication>()
        .SingleInstance();

    /// <inheritdoc />
    public void OnConfigureServices(IServiceProvider provider)
    { }
}