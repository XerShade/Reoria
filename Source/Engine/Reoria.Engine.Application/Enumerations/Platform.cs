namespace Reoria.Engine.Application.Enumerations;

/// <summary>
/// Defines flags for supported platforms of the application.
/// </summary>
[Flags]
public enum Platform
{
    /// <summary>
    /// Defines the flag value for an application running on the cross-platform desktop device.
    /// </summary>
    Desktop = 1 << 0,
    /// <summary>
    /// Defines the flag value for an application running on a windows device.
    /// </summary>
    Windows = 1 << 1,
    /// <summary>
    /// Defines the flag value for an application running on a iOS device.
    /// </summary>
    iOS = 1 << 2,
    /// <summary>
    /// Defines the flag value for an application running on an android device.
    /// </summary>
    Android = 1 << 3,
    /// <summary>
    /// Defines the flag value for an application running on a server.
    /// </summary>
    Server = 1 << 4,

    /// <summary>
    /// Defines the flag value for an application running on any platform.
    /// </summary>
    /// <remarks>This has the same expected functionality and behaviour as the <see cref="All"/> flag.</remarks>
    Any = 0,
    /// <summary>
    /// Defines the flag value for all supported platforms.
    /// </summary>
    /// <remarks>This has the same expected functionality and behaviour as the <see cref="Any"/> flag.</remarks>
    All = Desktop | Windows | iOS | Android | Server
}

/// <summary>
/// Defines extension methods for the <see cref="Platform"/> enumeration.
/// </summary>
public static class PlatformExtensions
{
    /// <summary>
    /// Checks if the injector platform matches the current platform.
    /// </summary>
    /// <param name="injectorPlatform">The platform of the injector.</param>
    /// <param name="currentPlatform">The current platform.</param>
    /// <returns>True if the injector platform matches the current platform, false otherwise.</returns>
    public static bool Matches(this Platform injectorPlatform, Platform currentPlatform)
        => injectorPlatform == Platform.Any || (injectorPlatform & currentPlatform) != 0;

    /// <summary>
    /// Checks if the value has a single flag set.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value has a single flag set, false otherwise.</returns>
    public static bool HasSingleFlag(this Platform value)
    {
        int v = (int)value;
        return v != 0 && (v & (v - 1)) == 0;
    }
}