using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines the properties and methods for an application context.
/// </summary>
/// <param name="platform">The platform that the application is running on.</param>
public sealed class AppBootContext(Platform platform)
{
    /// <summary>
    /// Gets the platform that the application is running on.
    /// </summary>
    public Platform Platform { get; init; } = platform;
}