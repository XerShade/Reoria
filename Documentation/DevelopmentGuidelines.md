# Development Guidelines

This document provides comprehensive guidelines for contributing to and developing the Reoria project. It covers coding standards, architectural patterns, and best practices to ensure consistency and quality across the codebase.

## Table of Contents

- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Architecture Patterns](#architecture-patterns)
- [Documentation Standards](#documentation-standards)
- [Testing Guidelines](#testing-guidelines)
- [Performance Considerations](#performance-considerations)
- [Security Best Practices](#security-best-practices)
- [Platform-Specific Development](#platform-specific-development)

## Project Structure

### Directory Organization

```
Reoria/
├── Applications/           # Platform-specific application entry points
│   ├── Reoria.Client.Android/    # Android client application
│   ├── Reoria.Client.Desktop/   # Desktop client application
│   ├── Reoria.Client.iOS/        # iOS client application
│   ├── Reoria.Client.Windows/   # Windows client application
│   └── Reoria.Server/           # Server application
├── Source/                # Core source code libraries
│   ├── Client/           # Client-specific components
│   ├── Engine/           # Game engine and framework
│   ├── Game/             # Game logic and entities
│   └── Server/           # Server-specific components
├── Build/              # Build configurations and MSBuild props
├── Documentation/       # Technical documentation
└── docker-compose.yml   # Container deployment configuration
```

### Naming Conventions

- **Projects**: Use `Reoria.Component.Platform` format (e.g., `Reoria.Client.Desktop`)
- **Namespaces**: Match project structure (e.g., `Reoria.Client.Core.Application`)
- **Files**: Use PascalCase for class files, kebab-case for documentation files

## Coding Standards

### C# Standards

- **.NET Version**: Target .NET 8.0 or later
- **Language Version**: Use latest C# features with consideration for compatibility
- **Code Style**: Follow Microsoft C# coding conventions
- **Nullable Reference Types**: Enable nullable reference types throughout

#### Example Class Structure

```csharp
using Microsoft.Extensions.Logging;

namespace Reoria.Engine.Application;

/// <summary>
/// Brief description of the class's purpose.
/// </summary>
/// <remarks>
/// Detailed information about usage, implementation details,
/// and any important considerations for developers.
/// </remarks>
public class ExampleClass
{
    private readonly ILogger<ExampleClass> logger;

    /// <summary>
    /// Initializes a new instance of the ExampleClass.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ExampleClass(ILogger<ExampleClass> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs the primary operation of this class.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    /// <remarks>
    /// Additional context about when and how to use this method.
    /// Include performance characteristics or side effects.
    /// </remarks>
    public async Task<OperationResult> PerformOperationAsync()
    {
        this.logger.LogDebug("Starting operation...");
        
        // Implementation here
        
        this.logger.LogDebug("Operation completed successfully.");
        return OperationResult.Success;
    }
}
```

### XML Documentation Standards

All public APIs must have comprehensive XML documentation:

- **Summary**: Brief description of purpose
- **Remarks**: Detailed usage information and implementation notes
- **Parameters**: Document all parameters with type and purpose
- **Returns**: Describe return values and possible states
- **Exceptions**: Document all exceptions that can be thrown
- **Examples**: Provide usage examples for complex APIs

## Architecture Patterns

### Dependency Injection

Use Autofac for dependency injection with the following patterns:

#### Service Registration

```csharp
// In injector classes
public class ExampleInjector : IBootStrapServicesInjector
{
    public void OnBuildServices(ContainerBuilder builder)
    {
        // Register services with appropriate lifetimes
        builder.RegisterType<ExampleService>()
            .As<IExampleService>()
            .SingleInstance();
            
        builder.RegisterType<TransientService>()
            .As<ITransientService>()
            .InstancePerLifetime();
    }
}
```

#### Service Resolution

```csharp
// In consuming classes
public class ConsumerClass
{
    private readonly IExampleService exampleService;

    public ConsumerClass(IExampleService exampleService)
    {
        this.exampleService = exampleService;
    }
}
```

### Game Loop Architecture

The game loop uses a phase-based architecture:

```csharp
public class CustomGameLoopPhase : IGameLoopPhase
{
    public string Name => "CustomPhase";
    public int Priority => 100;
    public bool IsEnabled => true;
    public bool IsAsync => false;

    public Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken)
    {
        // Phase implementation
        return Task.CompletedTask;
    }
}
```

### Server-Authoritative Architecture

Follow the server-authoritative multiplayer pattern:

1. **Server Authority**: All game state changes must be validated and processed by server
2. **Client Prediction**: Clients may predict locally but must accept server corrections
3. **Security Validation**: Use `SecurityValidator` for all client requests
4. **Entity Ownership**: Track entity ownership through `EntityOwnershipService`

## Documentation Standards

### Code Documentation

- **Public APIs**: Must have complete XML documentation
- **Internal APIs**: Should have XML documentation for complex logic
- **Comments**: Use `//` for single-line, `/* */` for multi-line
- **TODO Comments**: Use format `// TODO: Description - Owner - Date`

### File Headers

Add file headers to complex files:

```csharp
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExampleService.cs" company="Reoria">
//   Copyright (c) Reoria. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
```

## Testing Guidelines

### Unit Testing

- **Framework**: Use xUnit for unit tests
- **Naming**: Test classes should end with `Tests` (e.g., `ExampleServiceTests`)
- **Structure**: Arrange-Act-Assert pattern

```csharp
public class ExampleServiceTests
{
    [Fact]
    public void PerformOperation_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var service = new ExampleService(loggerMock.Object);
        
        // Act
        var result = service.PerformOperation();
        
        // Assert
        Assert.Equal(OperationResult.Success, result);
    }
}
```

### Integration Testing

- **Database**: Use in-memory database for tests
- **Network**: Mock network components for integration tests
- **Fixtures**: Use test fixtures for common test data

## Performance Considerations

### Memory Management

- **Dispose Pattern**: Implement `IDisposable` for resources that need cleanup
- **Weak References**: Use for event handlers to prevent memory leaks
- **Object Pooling**: Consider object pooling for frequently created/destroyed objects

### Threading

- **Thread Safety**: Use proper locking for shared state
- **Async/Await**: Use async patterns for I/O operations
- **Cancellation Tokens**: Support cancellation for long-running operations

### Game Loop Performance

- **Fixed Updates**: Use fixed timestep for physics and game logic
- **Delta Time**: Use delta time for frame-rate independent updates
- **Batching**: Batch similar operations to reduce draw calls

## Security Best Practices

### Input Validation

```csharp
public void ProcessClientInput(ClientInput input)
{
    // Always validate client input
    var validationResult = this.securityValidator.ValidateAction(player, "move", input);
    
    if (!validationResult.IsSuccess)
    {
        this.logger.LogWarning("Security violation: {Error}", validationResult.ErrorMessage);
        return;
    }
    
    // Process validated input
}
```

### Permission System

Use hierarchical permission strings:

- `action:move` - Movement permissions
- `action:chat` - Chat permissions
- `entity:own:character` - Entity ownership
- `command:admin` - Administrative commands

### Error Handling

- **Logging**: Log all security violations and errors
- **Graceful Degradation**: Handle failures without crashing
- **User Feedback**: Provide clear error messages to users

## Platform-Specific Development

### Client Platforms

#### Windows/Desktop
- Full keyboard and mouse input support
- Window management capabilities
- File system access with standard permissions

#### Android
- Touch input and accelerometer support
- Asset loading through Android asset system
- Activity lifecycle management

#### iOS
- Touch input and gesture support
- iOS-specific file system handling
- Application delegate pattern

### Server Platform
- Console-based operation
- Network socket management
- Database integration support

## Build and Deployment

### Local Development

```bash
# Build entire solution
dotnet build Reoria.slnx

# Run specific application
dotnet run --project Applications/Reoria.Client.Desktop
dotnet run --project Applications/Reoria.Server
```

### Docker Deployment

```bash
# Build and run server with Docker
docker-compose up --build

# Run in detached mode
docker-compose up -d
```

## Contributing Guidelines

### Pull Request Process

1. **Fork**: Create fork of the repository
2. **Branch**: Create feature branch from main
3. **Develop**: Implement changes following these guidelines
4. **Test**: Ensure all tests pass and code is documented
5. **Submit**: Create pull request with clear description

### Code Review Checklist

- [ ] Code follows naming conventions
- [ ] Public APIs have XML documentation
- [ ] Tests are included and passing
- [ ] No compiler warnings
- [ ] Performance impact considered
- [ ] Security implications reviewed
- [ ] Platform compatibility verified

## Resources

### Documentation References

- [Server Authoritative Multiplayer System](ServerAuthoritativeMultiplayer.md)
- [MonoGame Documentation](https://docs.monogame.net/)
- [.NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Tools and Extensions

Recommended Visual Studio extensions:
- **XML Documentation Comments**: For better IntelliSense support
- **Code Metrics**: For maintaining code quality
- **Git Extensions**: For improved source control integration

---

This document should be updated as the project evolves. Contributors should suggest improvements to these guidelines based on their experience developing with the Reoria codebase.
