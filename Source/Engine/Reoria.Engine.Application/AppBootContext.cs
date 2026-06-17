using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application;

/// <summary>
/// Defines the properties and methods for an application context.
/// </summary>
/// <param name="platform">The platform that the application is running on.</param>
/// <param name="args">The command-line arguments passed to the application.</param>
public sealed class AppBootContext(Platform platform, string[] args)
{
    /// <summary>
    /// Gets the platform that the application is running on.
    /// </summary>
    public Platform Platform { get; init; } = platform;
    /// <summary>
    /// Gets the command-line arguments passed to the application.
    /// </summary>
    public string[] Args { get; init; } = args;
}