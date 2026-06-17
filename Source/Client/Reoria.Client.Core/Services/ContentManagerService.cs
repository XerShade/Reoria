using Autofac;
using Microsoft.Xna.Framework.Content;
using Reoria.Client.Core.Injectors;
using Reoria.Client.Core.Services.Interfaces;
using Reoria.Engine.Application.Enumerations;

namespace Reoria.Client.Core.Services;

public class ContentManagerService : IContentManagerService, ILifeCycleLoadContentInjector
{
    public string Name
        => "Content Manager Service";

    public string Description
        => "Adds support for configuring the content manager and loading content.";

    public Type[] Dependencies
        => [];

    public Platform Platform
        => Platform.All & ~Platform.Server;

    public ContentManager? ContentManager { get; private set; }

    public void OnLoadContent(ContainerBuilder services, ContentManager contentManager)
    {
        // Store the provided content manager.
        this.ContentManager = contentManager;

        // Register the content manager with proper lifetime
        _ = services.RegisterInstance<ContentManager>(contentManager)
            .Keyed<ContentManager>("ContentManager")
            .As<ContentManager>()
            .SingleInstance();
    }
}