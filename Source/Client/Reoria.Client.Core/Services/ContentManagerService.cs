using Autofac;
using Microsoft.Xna.Framework.Content;
using Reoria.Client.Core.Services.Interfaces;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Phases;

namespace Reoria.Client.Core.Services;

/// <summary>
/// Game loop phase participant that provides content manager management.
/// </summary>
/// <remarks>
/// This phase participant handles content manager initialization during content loading.
/// For content loading, it uses method parameters (no DI dependencies).
/// </remarks>
public class ContentManagerService : IContentManagerService, IGameLoadContent
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