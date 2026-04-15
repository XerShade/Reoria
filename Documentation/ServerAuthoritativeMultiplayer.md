# Server Authoritative Multiplayer System

This document provides a comprehensive guide to the server authoritative multiplayer system designed for Reoria. The system handles player authentication, permissions, entity ownership, and security validation.

## System Overview

The system consists of several key components:

### Server-Side Components
- **Session Management**: Basic session lifecycle (already existing)
- **Player Management**: Player identity, roles, and permissions
- **Security Validation**: Action, command, and entity access validation
- **Entity Ownership**: Tracking entity ownership and access rights

### Client-Side Components
- **Client Session Data**: Local tracking of player state and owned entities
- **Session Management**: Client-side session lifecycle management

## Architecture

### Core Classes

#### Server-Side
- `Player`: Represents a connected player with identity and permissions
- `PlayerManager`: Manages all connected players
- `SecurityValidator`: Validates all security-sensitive operations
- `EntityOwnershipService`: Manages entity ownership and access rights

#### Client-Side
- `ClientSessionData`: Local session data and entity ownership tracking
- `ClientSessionManager`: Manages client-side session lifecycle

## Usage Examples

### Server-Side Usage

#### 1. Player Authentication and Session Creation

```csharp
// When a client connects
var session = sessionManager.Open(peer);

// Create player with authentication data
var player = playerManager.CreatePlayer(
    username: "PlayerName",
    roles: new[] { "player", "member" },
    permissions: new[] { 
        "action:move", 
        "action:chat", 
        "entity:own:character",
        "command:help"
    },
    session: session
);
```

#### 2. Security Validation

```csharp
// Validate an action
var validationResult = securityValidator.ValidateAction(
    player, 
    "move", 
    new { direction = "north", distance = 5 }
);

if (!validationResult.IsSuccess)
{
    // Handle security violation
    logger.LogWarning($"Security violation: {validationResult.ErrorMessage}");
    return;
}

// Proceed with the action
```

#### 3. Entity Ownership Management

```csharp
// Create an entity and assign ownership
var entityId = Guid.NewGuid();
ownershipService.SetEntityOwner(entityId, player.PlayerId);
ownershipService.SetEntityTypeInfo(entityId, "character", new Dictionary<string, object>
{
    ["level"] = 1,
    ["class"] = "warrior"
});

// Validate entity access
var accessResult = securityValidator.ValidateEntityAccess(
    player, 
    entityId, 
    "modify"
);

if (accessResult.IsSuccess)
{
    // Allow entity modification
}
```

#### 4. Command Validation

```csharp
// Validate a command execution
var commandResult = securityValidator.ValidateCommand(
    player, 
    "spawn_item", 
    new object[] { "sword", 1 }
);

if (commandResult.IsSuccess)
{
    // Execute the command
    ExecuteSpawnItem(player, "sword", 1);
}
```

### Client-Side Usage

#### 1. Session Management

```csharp
// Create session when connecting to server
var sessionData = clientSessionManager.CreateSession(Guid.NewGuid());

// Initialize with server-provided authentication data
clientSessionManager.InitializeSession(
    playerId: serverProvidedPlayerId,
    username: serverProvidedUsername,
    roles: serverProvidedRoles,
    permissions: serverProvidedPermissions
);
```

#### 2. Entity Ownership Tracking

```csharp
// When server assigns entity ownership
clientSessionManager.AddOwnedEntity(entityId);

// Check local ownership before sending requests
if (clientSessionManager.OwnsEntity(entityId))
{
    // Send entity modification request to server
    networkManager.SendEntityModification(entityId, modifications);
}

// When server removes entity ownership
clientSessionManager.RemoveOwnedEntity(entityId);
```

#### 3. Permission Checking (Client-Side Optimization)

```csharp
// Check permissions locally to avoid unnecessary server requests
if (clientSessionManager.HasPermission("action:move"))
{
    // Show movement UI
    ShowMovementControls();
}
```

## Permission System

The permission system uses a hierarchical string format:

### Action Permissions
- `action:move` - Allow movement
- `action:chat` - Allow chatting
- `action:trade` - Allow trading

### Command Permissions
- `command:help` - Allow help command
- `command:spawn_item` - Allow item spawning
- `command:kick:*` - Allow all kick commands

### Entity Permissions
- `entity:own:character` - Allow owning character entities
- `entity:access:all` - Allow access to all entities (admin)
- `entity:public:read` - Allow reading public entities

### Player Interaction Permissions
- `player:interaction:trade` - Allow trading with other players
- `player:interaction:message` - Allow messaging other players

## Integration with Existing Session System

The new player management system builds upon your existing session system:

1. **Session Creation**: When a client connects, create a session as before
2. **Player Creation**: After authentication, create a player associated with the session
3. **Security Validation**: Use the security validator for all operations
4. **Entity Ownership**: Track entity ownership for ECS integration

## Future ECS Integration

The system is designed to work with MonoGame.Extended.ECS:

### Server-Side ECS Integration
```csharp
// When creating ECS entities
var ecsEntity = world.CreateEntity();
ecsEntity.Set(new TransformComponent(position));
ecsEntity.Set(new OwnershipComponent(player.PlayerId));

// Update ownership service
ownershipService.SetEntityOwner(ecsEntityId, player.PlayerId);
```

### Client-Side ECS Integration
```csharp
// When receiving entity updates
if (clientSessionManager.OwnsEntity(entityId))
{
    // Create local ECS entity with ownership
    var localEntity = world.CreateEntity();
    localEntity.Set(new OwnershipComponent(true)); // Local ownership
    localEntity.Set(new LocalControlComponent()); // Enable local control
}
```

## Security Best Practices

1. **Never Trust Client Input**: Always validate on the server
2. **Use Principle of Least Privilege**: Grant minimal necessary permissions
3. **Validate All Operations**: Use SecurityValidator for all sensitive operations
4. **Audit Security Events**: Log security violations for monitoring
5. **Session Timeout**: Implement session timeout for inactive connections

## Thread Safety

All server-side components are designed to be thread-safe:
- PlayerManager uses locking for concurrent access
- EntityOwnershipService uses locking for ownership operations
- SecurityValidator is stateless and thread-safe

## Error Handling

The system provides detailed error information:
- SecurityValidationResult includes error codes and messages
- Use specific error codes for different violation types
- Log security violations for monitoring and debugging

## Performance Considerations

1. **Permission Caching**: Consider caching permission checks for frequently accessed permissions
2. **Entity Lookup**: EntityOwnershipService uses dictionary lookups for O(1) performance
3. **Session Cleanup**: Implement periodic cleanup of inactive sessions and entities

## Configuration

The system supports dependency injection through Autofac:
- Server components are registered in PlayersInjector
- Client components are registered in ClientSessionInjector
- Services are properly scoped for singleton/per-dependency usage

This system provides a solid foundation for server authoritative multiplayer with proper security, ownership management, and future ECS integration capabilities.
