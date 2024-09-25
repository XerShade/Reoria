using System;

namespace Reoria.Godot.Source.Enumerations;

/// <summary>
/// Represents directional flags for movement or facing in a 2D space.
/// </summary>
[Flags]
public enum Direction : byte
{
    /// <summary>
    /// No direction.
    /// </summary>
    None = 1 << 0,

    /// <summary>
    /// Direction indicating movement or facing upward.
    /// </summary>
    Up = 1 << 1,
    /// <summary>
    /// Direction indicating movement or facing downward.
    /// </summary>
    Down = 1 << 2,
    /// <summary>
    /// Direction indicating movement or facing to the left.
    /// </summary>
    Left = 1 << 3,
    /// <summary>
    /// Direction indicating movement or facing to the right.
    /// </summary>
    Right = 1 << 4,

    /// <summary>
    /// Combination of all cardinal directions (Up, Down, Left, Right).
    /// </summary>
    All = Up | Down | Left | Right
}
