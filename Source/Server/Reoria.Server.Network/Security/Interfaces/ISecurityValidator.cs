namespace Reoria.Server.Network.Security.Interfaces;

/// <summary>
/// Defines the contract for security validation in the server authoritative multiplayer system.
/// </summary>
public interface ISecurityValidator
{
    /// <summary>
    /// Validates whether a player has permission to perform a specific action.
    /// </summary>
    /// <param name="player">The player attempting the action.</param>
    /// <param name="action">The action being attempted.</param>
    /// <param name="context">Additional context for the validation.</param>
    /// <returns>The result of the security validation.</returns>
    SecurityValidationResult ValidateAction(Players.Player player, string action, object? context = null);

    /// <summary>
    /// Validates whether a player has permission to execute a specific command.
    /// </summary>
    /// <param name="player">The player attempting to execute the command.</param>
    /// <param name="command">The command being executed.</param>
    /// <param name="arguments">The arguments for the command.</param>
    /// <returns>The result of the security validation.</returns>
    SecurityValidationResult ValidateCommand(Players.Player player, string command, object?[]? arguments = null);

    /// <summary>
    /// Validates whether a player has ownership or access rights to a specific entity.
    /// </summary>
    /// <param name="player">The player attempting to access the entity.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="accessType">The type of access being requested.</param>
    /// <returns>The result of the security validation.</returns>
    SecurityValidationResult ValidateEntityAccess(Players.Player player, Guid entityId, string accessType);

    /// <summary>
    /// Validates whether a player can interact with another player.
    /// </summary>
    /// <param name="player">The player initiating the interaction.</param>
    /// <param name="targetPlayer">The target player.</param>
    /// <param name="interactionType">The type of interaction.</param>
    /// <returns>The result of the security validation.</returns>
    SecurityValidationResult ValidatePlayerInteraction(Players.Player player, Players.Player targetPlayer, string interactionType);
}

/// <summary>
/// Represents the result of a security validation operation.
/// </summary>
public class SecurityValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the error code if validation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets additional context about the validation result.
    /// </summary>
    public Dictionary<string, object> Context { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static SecurityValidationResult Success() => new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed validation result with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing why validation failed.</param>
    /// <param name="errorCode">The error code for the failure.</param>
    /// <returns>A failed validation result.</returns>
    public static SecurityValidationResult Failure(string errorMessage, string? errorCode = null)
        => new() { IsSuccess = false, ErrorMessage = errorMessage, ErrorCode = errorCode };

    /// <summary>
    /// Creates a failed validation result for insufficient permissions.
    /// </summary>
    /// <param name="requiredPermission">The permission that was required.</param>
    /// <returns>A failed validation result for insufficient permissions.</returns>
    public static SecurityValidationResult InsufficientPermission(string requiredPermission)
        => Failure($"Insufficient permissions. Required: {requiredPermission}", "INSUFFICIENT_PERMISSIONS");

    /// <summary>
    /// Creates a failed validation result for unauthorized access.
    /// </summary>
    /// <param name="resource">The resource that was being accessed.</param>
    /// <returns>A failed validation result for unauthorized access.</returns>
    public static SecurityValidationResult Unauthorized(string resource)
        => Failure($"Unauthorized access to resource: {resource}", "UNAUTHORIZED_ACCESS");
}