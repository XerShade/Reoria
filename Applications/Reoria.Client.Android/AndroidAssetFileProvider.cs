#if ANDROID
using Android.Content.Res;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Reoria.Engine.Core.Configuration.Providers;

/// <summary>
/// Provides an <see cref="IFileProvider"/> implementation that reads files from
/// the Android application APK assets using <see cref="AssetManager"/>.
/// </summary>
/// <remarks>
/// This provider enables standard .NET configuration extensions such as
/// <c>AddJsonFile()</c> to function on Android by redirecting file access
/// to packaged Android assets.
/// </remarks>
/// <param name="assetManager">
/// The <see cref="AssetManager"/> instance used to access APK asset files.
/// </param>
public sealed class AndroidAssetFileProvider(AssetManager assetManager) : IFileProvider
{
    /// <summary>
    /// Gets the Android <see cref="AssetManager"/> used to access packaged assets.
    /// </summary>
    private AssetManager AssetManager { get; init; } = assetManager;

    /// <summary>
    /// Returns directory contents for the specified subpath.
    /// </summary>
    /// <param name="subpath">The relative path within the asset directory.</param>
    /// <returns>
    /// Always returns <see cref="NotFoundDirectoryContents.Singleton"/> because
    /// Android assets do not expose directory enumeration in this provider.
    /// </returns>
    public IDirectoryContents GetDirectoryContents(string subpath)
        => NotFoundDirectoryContents.Singleton;

    /// <summary>
    /// Returns file information for the specified subpath.
    /// </summary>
    /// <param name="subpath">The relative path to the asset file.</param>
    /// <returns>
    /// An <see cref="IFileInfo"/> implementation that represents the Android asset file.
    /// </returns>
    public IFileInfo GetFileInfo(string subpath)
        => new AndroidAssetFileInfo(this.AssetManager, Normalize(subpath));

    /// <summary>
    /// Creates a change token for the specified filter.
    /// </summary>
    /// <param name="filter">The file filter pattern.</param>
    /// <returns>
    /// Always returns <see cref="NullChangeToken.Singleton"/> because
    /// Android APK assets are immutable at runtime and do not support change tracking.
    /// </returns>
    public IChangeToken Watch(string filter)
        => NullChangeToken.Singleton;

    /// <summary>
    /// Normalizes a file path for Android asset resolution.
    /// </summary>
    /// <param name="path">The original file path.</param>
    /// <returns>
    /// A normalized path with leading slashes removed and backslashes replaced
    /// with forward slashes to match Android asset conventions.
    /// </returns>
    private static string Normalize(string path)
        => path.TrimStart('/').Replace("\\", "/");
}
#endif