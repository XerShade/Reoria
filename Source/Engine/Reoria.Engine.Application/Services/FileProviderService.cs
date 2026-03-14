using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Reoria.Engine.Application.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Reoria.Engine.Application.Services;

/// <summary>
/// Defines a service that provides access to and management of a collection of <see cref="IFileProvider"/> instances.
/// </summary>
public partial class FileProviderService : IFileProviderService, IFileProvider
{
    /// <summary>
    /// A collection of <see cref="IFileProvider"/> instances.
    /// </summary>
    protected virtual List<IFileProvider> Providers { get; init; } = [];

    /// <inheritdoc />
    public virtual void AddFileProvider<TProvider>(TProvider provider)
        where TProvider : class, IFileProvider
        // Add the specified provider.
        => this.Providers.Add(provider);

    /// <inheritdoc />
    public virtual void AddFileProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    TProvider>(params object[] args)
        where TProvider : class, IFileProvider
        // Add a new instance of the specified provider.
        => this.Providers.Add((IFileProvider)Activator.CreateInstance(typeof(TProvider), args)!);

    /// <inheritdoc />
    public virtual void RemoveFileProvider<TProvider>(TProvider provider)
        where TProvider : class, IFileProvider
        // Remove the specified provider.
        => this.Providers.Remove(provider);

    /// <inheritdoc />
    public virtual void RemoveFileProvider<TProvider>()
        where TProvider : class, IFileProvider
        // Remove all providers of the specified type.
        => this.Providers.RemoveAll(p => p is TProvider);

    /// <inheritdoc />
    public virtual bool HasProvider<TProvider>()
        where TProvider : class, IFileProvider
        // Return true if the provider of the specified type exists.
        => this.Providers.Any(p => p is TProvider);

    /// <inheritdoc />
    public virtual TProvider? GetProvider<TProvider>()
        where TProvider : class, IFileProvider
        // Return the first provider of the specified type.
        => this.Providers.OfType<TProvider>().FirstOrDefault();

    /// <inheritdoc />
    public virtual IEnumerable<TProvider> GetProviders<TProvider>()
        where TProvider : class, IFileProvider
        // Return the providers of the specified type.
        => this.Providers.OfType<TProvider>();

    /// <inheritdoc />
    public virtual IEnumerable<IFileProvider> GetProviders()
        // Return the providers.
        => this.Providers;

    /// <inheritdoc />
    public virtual IFileInfo GetFileInfo(string subpath)
    {
        // Iterate over the providers.
        foreach (IFileProvider provider in this.Providers)
        {
            // Get the file info for the provider.
            IFileInfo file = provider.GetFileInfo(subpath);

            // Check if the file info exists.
            if (file.Exists)
            {
                // Return the file info.
                return file;
            }
        }

        // Get the file info for the default provider.
        return IFileProviderService.FileProvider.GetFileInfo(subpath);
    }

    /// <inheritdoc />
    public virtual IDirectoryContents GetDirectoryContents(string subpath)
    {
        // Create a collection of directory contents.
        List<IFileInfo> results = [];

        // Iterate over the providers.
        foreach (IFileProvider provider in this.Providers)
        {
            // Get the directory contents for the provider.
            IDirectoryContents contents = provider.GetDirectoryContents(subpath);

            // Check if the directory contents exist.
            if (contents.Exists)
            {
                // Add the directory contents to the collection.
                results.AddRange(contents);
            }
        }

        // Get the directory contents for the default provider.
        IDirectoryContents rootContents = IFileProviderService.FileProvider.GetDirectoryContents(subpath);

        // Check if the directory contents exist.
        if (rootContents.Exists)
        {
            // Add the directory contents to the collection.
            results.AddRange(rootContents);
        }

        // Return the directory contents.
        return new IFileProviderService.EnumerableDirectoryContents(results);
    }

    /// <inheritdoc />
    public virtual IChangeToken Watch(string filter)
    {
        // Create a collection of change tokens.
        List<IChangeToken> tokens = [];

        // Iterate over the providers.
        foreach (IFileProvider provider in this.Providers)
        {
            // Add the change token for the provider.
            tokens.Add(provider.Watch(filter));
        }

        // Add the change token for the default provider.
        tokens.Add(IFileProviderService.FileProvider.Watch(filter));

        // Return a composite change token.
        return new CompositeChangeToken(tokens);
    }
}