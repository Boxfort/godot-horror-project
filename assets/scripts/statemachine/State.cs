using System;
using Godot;

public abstract class State<T, N>
    where T : Enum
    where N : Node
{
    public abstract T StateType { get; }
    public abstract void Enter(N node);

    /// Update the current state
    /// <returns>
    /// A tuple of next state to travel to, and whether the next state is a sub-state of the current state.
    /// If the state to travel to is the "None" state then this state will not exit.
    /// If the state to travel to is a sub-state then the sub-state will be entered without exiting the current state, and the sub-state will return to this state once complete.
    /// </returns>
    public abstract T Update(N node, double delta);
    public abstract void Exit(N node);
}
