using Reoria.Engine.Application.Enumerations;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Base interface for all injector types providing common metadata.
/// </summary>
/// <remarks>
/// All injector interfaces (bootstrap, application, and game loop) inherit from this base interface.
/// This provides a consistent way to identify, describe, and manage dependencies for all injectors.
/// </remarks>
public interface IInjector
{
    /// <summary>
    /// Gets the name of the injector.
    /// </summary>
    /// <remarks>
    /// This name should be descriptive and unique for identification purposes.
    /// It's often used in logging and debugging to track which injectors are active.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Gets a description of what the injector does.
    /// </summary>
    /// <remarks>
    /// This should provide a clear explanation of the injector's purpose
    /// and what functionality it provides to the application or game loop.
    /// </remarks>
    string Description { get; }

    /// <summary>
    /// Gets the dependencies of the injector represented as a list of types.
    /// </summary>
    /// <remarks>
    /// This array contains the types of other injectors that must be initialized
    /// before this injector can be safely used. The system uses this for dependency
    /// resolution and proper initialization order.
    /// </remarks>
    Type[] Dependencies { get; }

    /// <summary>
    /// Gets the platform(s) that the injector is compatible with.
    /// </summary>
    /// <remarks>
    /// This property determines on which platforms (Windows, Android, iOS, Server, etc.)
    /// the injector will be loaded and executed. Injectors are only loaded on compatible platforms.
    /// </remarks>
    Platform Platform { get; }
}