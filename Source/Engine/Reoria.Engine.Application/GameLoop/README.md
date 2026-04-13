# Game Loop System

This directory contains the extensible game loop system for the Reoria Engine. The system provides a modular, phase-based approach to game loop execution that makes it easy to add, remove, and reorder game logic.

## Core Components

### IGameLoop
The main interface that defines the game loop contract. It manages the overall execution cycle and coordinates phases.

### IGameLoopPhase
Represents a single phase in the game loop. Each phase has:
- **Name**: Human-readable identifier
- **Priority**: Higher values execute first
- **IsEnabled**: Can be used to temporarily disable phases
- **ExecuteAsync**: The main execution method

### IGameLoopContext
Provides context information for each tick, including:
- Game time information
- Tick number
- Whether this is a fixed update
- Custom properties for sharing data between phases

## Built-in Phases

### NetworkUpdatePhase (Priority: 100)
Handles network socket updates. Runs first to ensure network state is up-to-date.

### InjectorExecutionPhase (Priority: 50)
Executes all application injectors that implement update methods.

## Creating Custom Phases

To create a custom phase, implement `IGameLoopPhase`:

```csharp
public class CustomPhase : IGameLoopPhase
{
    public string Name => "Custom Logic";
    public int Priority => 75; // Between network and injector phases
    public bool IsEnabled => true;

    public async Task ExecuteAsync(IGameLoopContext context, CancellationToken cancellationToken = default)
    {
        if (context.IsFixedUpdate)
        {
            // Fixed update logic (physics, etc.)
        }
        else
        {
            // Variable update logic (input, rendering, etc.)
        }
        
        await Task.CompletedTask;
    }
}
```

## Adding Phases to ServerApplication

In your `ServerApplication.InitializeGameLoop()` method, add custom phases:

```csharp
protected virtual IGameLoop InitializeGameLoop()
{
    var phases = new List<IGameLoopPhase>
    {
        new NetworkUpdatePhase(this.LoggerFactory.CreateLogger<NetworkUpdatePhase>(), this.Socket),
        new CustomGameLoopPhase(this.LoggerFactory.CreateLogger<CustomGameLoopPhase>()), // Your custom phase
        new InjectorExecutionPhase(this.LoggerFactory.CreateLogger<InjectorExecutionPhase>(), this)
    };

    return new DefaultGameLoop(this.LoggerFactory.CreateLogger<DefaultGameLoop>(), phases);
}
```

## Phase Execution Order

Phases are executed in descending priority order:
1. NetworkUpdatePhase (100) - Network updates
2. Custom phases (75) - Your custom logic
3. InjectorExecutionPhase (50) - Injector updates

## Injector Integration

Injectors can now implement game loop methods:
- `OnApplicationStart()` - Called when application starts
- `OnApplicationStop()` - Called when application stops  
- `OnVariableUpdate(GameTime)` - Variable update calls
- `OnFixedUpdate(GameTime)` - Fixed update calls

## Benefits

1. **Modularity**: Each piece of logic is isolated in its own phase
2. **Extensibility**: Easy to add new functionality without modifying existing code
3. **Testability**: Phases can be unit tested independently
4. **Reordering**: Priority system allows easy reordering of execution
5. **Conditional Execution**: Phases can be enabled/disabled at runtime
6. **Async Support**: Phases support asynchronous execution
7. **Context Sharing**: Phases can share data through the context properties

## Best Practices

1. Keep phases focused on single responsibilities
2. Use descriptive names for phases
3. Set appropriate priorities based on dependencies
4. Use the context properties to share data between phases
5. Implement proper error handling in phase execution
6. Consider performance impact when adding many phases
