using System;
using Godot;

public abstract class SubState<T, N> : State<T, N>
    where T : Enum
    where N : Node
{
    protected T previousState;
    public T PreviousState
    {
        get => previousState;
        set => previousState = value;
    }
}
