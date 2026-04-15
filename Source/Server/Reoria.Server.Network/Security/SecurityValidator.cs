using Reoria.Server.Network.Security.Interfaces;

namespace Reoria.Server.Network.Security;

/// <summary>
/// Default implementation of ISecurityValidator that provides comprehensive security validation
/// for server authoritative multiplayer operations.
/// </summary>
public class SecurityValidator : ISecurityValidator
{
    private readonly IEntityOwnershipService ownershipService;

    /// <summary>
    /// Initializes a new instance of the SecurityValidator class.
    /// </summary>
    /// <param name="ownershipService">The entity ownership service for validating entity access.</param>
    public SecurityValidator(IEntityOwnershipService ownershipService)
    {
        this.ownershipService = ownershipService ?? throw new ArgumentNullException(nameof(ownershipService));
    }

    /// <summary>
    /// Validates whether a player has permission to perform a specific action.
    /// </summary>
    /// <param name="player">The player attempting the action.</param>
    /// <param name="action">The action being attempted.</param>
    /// <param name="context">Additional context for the validation.</param>
    /// <returns>The result of the security validation.</returns>
    public SecurityValidationResult ValidateAction(Players.Player player, string action, object? context = null)
    {
        if (player == null)
            return SecurityValidationResult.Failure("Player cannot be null.", "INVALID_PLAYER");

        if (string.IsNullOrWhiteSpace(action))
            return SecurityValidationResult.Failure("Action cannot be null or empty.", "INVALID_ACTION");

        // Update player's last activity
        player.UpdateLastActivity();

        // Check if player has the specific permission for this action
        var requiredPermission = $"action:{action}";
        if (!player.HasPermission(requiredPermission))
        {
            // Check for wildcard permissions
            var actionParts = action.Split(':');
            for (int i = actionParts.Length - 1; i > 0; i--)
            {
                var wildcardPermission = string.Join(":", actionParts.Take(i)) + ":*";
                if (player.HasPermission(wildcardPermission))
                    return SecurityValidationResult.Success();
            }

            return SecurityValidationResult.InsufficientPermission(requiredPermission);
        }

        // Additional context-based validation can be added here
        if (context != null)
        {
            // Example: Validate action context if needed
            if (context is Dictionary<string, object> contextDict)
            {
                if (contextDict.ContainsKey("target_entity_id") && contextDict["target_entity_id"] is Guid entityId)
                {
                    var entityAccessResult = ValidateEntityAccess(player, entityId, "action");
                    if (!entityAccessResult.IsSuccess)
                        return entityAccessResult;
                }
            }
        }

        return SecurityValidationResult.Success();
    }

    /// <summary>
    /// Validates whether a player has permission to execute a specific command.
    /// </summary>
    /// <param name="player">The player attempting to execute the command.</param>
    /// <param name="command">The command being executed.</param>
    /// <param name="arguments">The arguments for the command.</param>
    /// <returns>The result of the security validation.</returns>
    public SecurityValidationResult ValidateCommand(Players.Player player, string command, object?[]? arguments = null)
    {
        if (player == null)
            return SecurityValidationResult.Failure("Player cannot be null.", "INVALID_PLAYER");

        if (string.IsNullOrWhiteSpace(command))
            return SecurityValidationResult.Failure("Command cannot be null or empty.", "INVALID_COMMAND");

        // Update player's last activity
        player.UpdateLastActivity();

        // Check if player has the specific permission for this command
        var requiredPermission = $"command:{command}";
        if (!player.HasPermission(requiredPermission))
        {
            // Check for wildcard command permissions
            var commandParts = command.Split(' ');
            if (commandParts.Length > 0)
            {
                var baseCommand = commandParts[0];
                var wildcardPermission = $"command:{baseCommand}:*";
                if (player.HasPermission(wildcardPermission))
                    return SecurityValidationResult.Success();
            }

            return SecurityValidationResult.InsufficientPermission(requiredPermission);
        }

        // Validate command arguments if needed
        if (arguments != null && arguments.Length > 0)
        {
            // Example: Validate that certain commands require specific arguments
            if (command.StartsWith("entity:") && arguments.Length > 0 && arguments[0] is Guid entityId)
            {
                var entityAccessResult = ValidateEntityAccess(player, entityId, "command");
                if (!entityAccessResult.IsSuccess)
                    return entityAccessResult;
            }
        }

        return SecurityValidationResult.Success();
    }

    /// <summary>
    /// Validates whether a player has ownership or access rights to a specific entity.
    /// </summary>
    /// <param name="player">The player attempting to access the entity.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="accessType">The type of access being requested.</param>
    /// <returns>The result of the security validation.</returns>
    public SecurityValidationResult ValidateEntityAccess(Players.Player player, Guid entityId, string accessType)
    {
        if (player == null)
            return SecurityValidationResult.Failure("Player cannot be null.", "INVALID_PLAYER");

        if (entityId == Guid.Empty)
            return SecurityValidationResult.Failure("Entity ID cannot be empty.", "INVALID_ENTITY_ID");

        if (string.IsNullOrWhiteSpace(accessType))
            return SecurityValidationResult.Failure("Access type cannot be null or empty.", "INVALID_ACCESS_TYPE");

        // Update player's last activity
        player.UpdateLastActivity();

        // Check if player owns the entity
        if (ownershipService.IsEntityOwnedBy(entityId, player.PlayerId))
            return SecurityValidationResult.Success();

        // Check if player has admin permissions to access any entity
        if (player.HasPermission("entity:access:all"))
            return SecurityValidationResult.Success();

        // Check if player has specific access permission for this entity type
        var entityTypeInfo = ownershipService.GetEntityTypeInfo(entityId);
        if (entityTypeInfo != null)
        {
            var entityTypePermission = $"entity:{entityTypeInfo.Type}:{accessType}";
            if (player.HasPermission(entityTypePermission))
                return SecurityValidationResult.Success();
        }

        // Check if entity is shared or public
        if (ownershipService.IsEntityPublic(entityId))
        {
            var publicAccessPermission = $"entity:public:{accessType}";
            if (player.HasPermission(publicAccessPermission))
                return SecurityValidationResult.Success();
        }

        return SecurityValidationResult.Unauthorized($"Entity {entityId} for access type {accessType}");
    }

    /// <summary>
    /// Validates whether a player can interact with another player.
    /// </summary>
    /// <param name="player">The player initiating the interaction.</param>
    /// <param name="targetPlayer">The target player.</param>
    /// <param name="interactionType">The type of interaction.</param>
    /// <returns>The result of the security validation.</returns>
    public SecurityValidationResult ValidatePlayerInteraction(Players.Player player, Players.Player targetPlayer, string interactionType)
    {
        if (player == null)
            return SecurityValidationResult.Failure("Player cannot be null.", "INVALID_PLAYER");

        if (targetPlayer == null)
            return SecurityValidationResult.Failure("Target player cannot be null.", "INVALID_TARGET_PLAYER");

        if (string.IsNullOrWhiteSpace(interactionType))
            return SecurityValidationResult.Failure("Interaction type cannot be null or empty.", "INVALID_INTERACTION_TYPE");

        // Update player's last activity
        player.UpdateLastActivity();

        // Players can always interact with themselves
        if (player.PlayerId == targetPlayer.PlayerId)
            return SecurityValidationResult.Success();

        // Check if player has permission for this interaction type
        var requiredPermission = $"player:interaction:{interactionType}";
        if (!player.HasPermission(requiredPermission))
        {
            // Check for wildcard interaction permissions
            var wildcardPermission = "player:interaction:*";
            if (player.HasPermission(wildcardPermission))
                return SecurityValidationResult.Success();

            return SecurityValidationResult.InsufficientPermission(requiredPermission);
        }

        // Additional validation based on interaction type
        return interactionType.ToLower() switch
        {
            "trade" => ValidateTradeInteraction(player, targetPlayer),
            "party_invite" => ValidatePartyInviteInteraction(player, targetPlayer),
            "guild_invite" => ValidateGuildInviteInteraction(player, targetPlayer),
            "message" => ValidateMessageInteraction(player, targetPlayer),
            _ => SecurityValidationResult.Success()
        };
    }

    private SecurityValidationResult ValidateTradeInteraction(Players.Player player, Players.Player targetPlayer)
    {
        // Check if both players have trade permission
        if (!targetPlayer.HasPermission("player:interaction:trade"))
            return SecurityValidationResult.Failure("Target player does not allow trades.", "TARGET_NO_TRADE");

        return SecurityValidationResult.Success();
    }

    private SecurityValidationResult ValidatePartyInviteInteraction(Players.Player player, Players.Player targetPlayer)
    {
        // Check if target player accepts party invites
        if (!targetPlayer.HasPermission("player:interaction:party_invite"))
            return SecurityValidationResult.Failure("Target player does not accept party invites.", "TARGET_NO_PARTY_INVITE");

        return SecurityValidationResult.Success();
    }

    private SecurityValidationResult ValidateGuildInviteInteraction(Players.Player player, Players.Player targetPlayer)
    {
        // Check if target player accepts guild invites
        if (!targetPlayer.HasPermission("player:interaction:guild_invite"))
            return SecurityValidationResult.Failure("Target player does not accept guild invites.", "TARGET_NO_GUILD_INVITE");

        return SecurityValidationResult.Success();
    }

    private SecurityValidationResult ValidateMessageInteraction(Players.Player player, Players.Player targetPlayer)
    {
        // Check if target player accepts messages
        if (!targetPlayer.HasPermission("player:interaction:message"))
            return SecurityValidationResult.Failure("Target player does not accept messages.", "TARGET_NO_MESSAGES");

        return SecurityValidationResult.Success();
    }
}
