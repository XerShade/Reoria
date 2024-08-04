using Godot;
using System;

namespace Reoria.Nodes.Engine;

public partial class GameInstance : Node
{
    private static GameInstance instance;

    public static GameInstance Instance => instance is not null ? instance
                : throw new NullReferenceException("The game instance node has been lost, the game will now close.");

    public GameInstance()
    {
        if (instance is not null)
        {
            throw new InvalidOperationException("The game can not load more then one instance node, the game will now close.");
        }

        instance = this;
    }
}
