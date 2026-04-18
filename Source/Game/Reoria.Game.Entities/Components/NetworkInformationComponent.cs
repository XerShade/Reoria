namespace Reoria.Engine.Core.Components;

/// <summary>
/// Component that stores network-related information for entities in multiplayer scenarios.
/// </summary>
/// <remarks>
/// This component is essential for server-authoritative multiplayer architecture.
/// It tracks which player owns or controls a specific entity, enabling
/// proper access control and security validation. The OwnerId corresponds
/// to the player's unique identifier assigned by the server during authentication.
/// Entities with OwnerId = -1 are unassigned and typically represent
/// server-owned objects or entities awaiting ownership assignment.
/// </remarks>
public class NetworkInformationComponent
{
    /// <summary>
    /// Gets or sets the unique identifier of the player who owns this entity.
    /// </summary>
    /// <remarks>
    /// This ID corresponds to the Player.PlayerId from the server's player management system.
    /// When a client creates an entity, this value is -1 until the server
    /// assigns ownership. Only the owning player can modify or control the entity
    /// when server-authoritative security is enforced. This property is crucial
    /// for preventing unauthorized entity manipulation in multiplayer environments.
    /// </remarks>
    public int OwnerId { get; set; } = -1;
}