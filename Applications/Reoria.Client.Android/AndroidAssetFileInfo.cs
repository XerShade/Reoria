#if ANDROID
using Android.Content.Res;
using Microsoft.Extensions.FileProviders;

namespace Reoria.Engine.Core.Configuration.Providers;

/// <summary>
/// Represents file metadata and stream access for a file packaged
/// within the Android application APK assets.
/// </summary>
/// <remarks>
/// This implementation bridges Android's <see cref="AssetManager"/> file access
/// with the .NET <see cref="IFileInfo"/> abstraction so that configuration
/// providers and other file-based systems can operate normally.
/// </remarks>
/// <param name="assetManager">
/// The <see cref="AssetManager"/> used to access the asset file.
/// </param>
/// <param name="path">
/// The relative path to the asset within the APK.
/// </param>
internal sealed class AndroidAssetFileInfo(AssetManager assetManager, string path) : IFileInfo
{
    /// <summary>
    /// The Android asset manager used to open the file stream.
    /// </summary>
    private readonly AssetManager AssetManager = assetManager;

    /// <summary>
    /// The normalized path of the asset file within the APK.
    /// </summary>
    private readonly string Path = path;

    /// <summary>
    /// Gets a value indicating whether the asset file exists.
    /// </summary>
    /// <remarks>
    /// Existence is determined by attempting to open the asset stream.
    /// </remarks>
    public bool Exists
    {
        get
        {
            try
            {
                using Stream stream = this.AssetManager.Open(this.Path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the length of the asset file in bytes.
    /// </summary>
    /// <remarks>
    /// The length is determined by opening the asset stream and
    /// reading its <see cref="Stream.Length"/> property.
    /// </remarks>
    public long Length
    {
        get
        {
            using Stream stream = this.AssetManager.Open(this.Path);
            return stream.Length;
        }
    }

    /// <summary>
    /// Gets the physical path of the file, if available.
    /// </summary>
    /// <remarks>
    /// Always returns <c>null</c> because Android assets do not have
    /// a physical filesystem path accessible at runtime.
    /// </remarks>
    public string PhysicalPath => null!;

    /// <summary>
    /// Gets the file name and extension of the asset.
    /// </summary>
    public string Name => System.IO.Path.GetFileName(this.Path);

    /// <summary>
    /// Gets the last modified timestamp of the file.
    /// </summary>
    /// <remarks>
    /// Always returns <see cref="DateTimeOffset.MinValue"/> because
    /// Android assets do not expose modification metadata.
    /// </remarks>
    public DateTimeOffset LastModified => DateTimeOffset.MinValue;

    /// <summary>
    /// Gets a value indicating whether the file represents a directory.
    /// </summary>
    /// <remarks>
    /// Always returns <c>false</c> because this implementation only
    /// represents individual asset files.
    /// </remarks>
    public bool IsDirectory => false;

    /// <summary>
    /// Creates and returns a readable stream for the asset file.
    /// </summary>
    /// <returns>A <see cref="Stream"/> for reading the asset contents.</returns>
    public Stream CreateReadStream()
        => this.AssetManager.Open(this.Path);
}
#endif