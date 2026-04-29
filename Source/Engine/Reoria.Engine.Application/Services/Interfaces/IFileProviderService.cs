using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace Reoria.Engine.Application.Services.Interfaces;

/// <summary>
/// Defines an abstraction contract for a service that provides access to a collection of <see cref="IFileProvider"/> instances.
/// </summary>
public partial interface IFileProviderService
{
    /// <summary>
    /// The default <see cref="IFileProvider"/> instance to fall back to.
    /// </summary>
    /// <remarks>The default value is <see cref="PhysicalFileProvider"/> and is accessed via the <see cref="FileProvider"/> property globally.</remarks>
    public static IFileProvider FileProvider { get; private set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());

    /// <summary>
    /// Changes the default <see cref="IFileProvider"/> instance to fall back to.
    /// </summary>
    /// <param name="provider">The new <see cref="IFileProvider"/> instance to fall back to.</param>
    /// <exception cref="NullReferenceException">Thrown if <paramref name="provider"/> is <see langword="null"/>.</exception>
    public static void SetFileProvider(IFileProvider provider)
        => FileProvider = provider ?? throw new ArgumentNullException(nameof(provider));

    /// <summary>
    /// Adds a new <see cref="IFileProvider"/> instance to the collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to add.</typeparam>
    /// <param name="args">The constructor arguments for the <see cref="IFileProvider"/> instance.</param>
    void AddFileProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(params object[] args) where TProvider : class, IFileProvider;
    /// <summary>
    /// Adds an existing <see cref="IFileProvider"/> instance to the collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to add.</typeparam>
    /// <param name="provider">The <see cref="IFileProvider"/> instance to add.</param>
    void AddFileProvider<TProvider>(TProvider provider) where TProvider : class, IFileProvider;
    /// <summary>
    /// Enumerates a directory at the given path, if any.
    /// </summary>
    /// <param name="subpath">The relative path that identifies the directory.</param>
    /// <returns>The contents of the directory.</returns>
    IDirectoryContents GetDirectoryContents(string subpath);
    /// <summary>
    /// Locates a file at the given path.
    /// </summary>
    /// <param name="subpath">The relative path that identifies the file.</param>
    /// <returns>The file information. Caller must check Exists property.</returns>
    IFileInfo GetFileInfo(string subpath);
    /// <summary>
    /// Returns the first <see cref="IFileProvider"/> instance for the specified type.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to get.</typeparam>
    /// <returns>The <see cref="IFileProvider"/> instance.</returns>
    TProvider? GetProvider<TProvider>() where TProvider : class, IFileProvider;
    /// <summary>
    /// Returns a collection of all <see cref="IFileProvider"/> instances.
    /// </summary>
    /// <returns>The collection of <see cref="IFileProvider"/> instances.</returns>
    IEnumerable<IFileProvider> GetProviders();
    /// <summary>
    /// Returns a collection of all <see cref="IFileProvider"/> instances of the specified type.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to get.</typeparam>
    /// <returns>The collection of <see cref="IFileProvider"/> instances.</returns>
    IEnumerable<TProvider> GetProviders<TProvider>() where TProvider : class, IFileProvider;
    /// <summary>
    /// Checks to see if the collection contains a <see cref="IFileProvider"/> instance of the specified type.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to check for.</typeparam>
    /// <returns>True if the collection contains a <see cref="IFileProvider"/> instance of the specified type; otherwise, false.</returns>
    bool HasProvider<TProvider>() where TProvider : class, IFileProvider;
    /// <summary>
    /// Removes a <see cref="IFileProvider"/> instance from the collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to remove.</typeparam>
    void RemoveFileProvider<TProvider>() where TProvider : class, IFileProvider;
    /// <summary>
    /// Removes the specified <see cref="IFileProvider"/> instance from the collection.
    /// </summary>
    /// <typeparam name="TProvider">The type of <see cref="IFileProvider"/> to remove.</typeparam>
    /// <param name="provider">The <see cref="IFileProvider"/> instance to remove.</param>
    void RemoveFileProvider<TProvider>(TProvider provider) where TProvider : class, IFileProvider;
    /// <summary>
    /// Creates an <see cref="IChangeToken"/> for the specified <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A filter string used to determine what files or folders to monitor. Examples: **/*.cs, *.*, subFolder/**/*.cshtml.</param>
    /// <returns>An <see cref="IChangeToken"/> that is notified when a file matching <paramref name="filter"/> is added, modified, or deleted.</returns>
    IChangeToken Watch(string filter);

    /// <summary>
    /// An implementation of <see cref="IDirectoryContents"/> that wraps an enumerable of <see cref="IFileInfo"/> instances.
    /// </summary>
    /// <param name="entries">The enumerable of <see cref="IFileInfo"/> instances.</param>
    public class EnumerableDirectoryContents(IEnumerable<IFileInfo> entries) : IDirectoryContents
    {
        /// <summary>
        /// The enumerable of <see cref="IFileInfo"/> instances.
        /// </summary>
        private readonly IEnumerable<IFileInfo> Entries = entries ?? [];

        /// <summary>
        /// Checks to see if the collection contains any <see cref="IFileInfo"/> instances.
        /// </summary>
        public bool Exists
            => this.Entries.Any();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IFileInfo> GetEnumerator()
            => this.Entries.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Entries.GetEnumerator();
    }
}