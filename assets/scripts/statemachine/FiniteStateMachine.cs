using System;
using System.Collections.Generic;
using Godot;

public abstract partial class FiniteStateMachine<T, N> : Node
    where N : Node
    where T : Enum
{
    [Signal]
    public delegate void ChangedStateEventHandler(string state);

    protected abstract T InitialState { get; }
    protected abstract T NoneState { get; }
    protected abstract Dictionary<T, State<T, N>> States { get; }
    public T CurrentState
    {
        get => currentState;
        set => ChangeState(value);
    }
    protected T currentState;
    N node;

    public override void _Ready()
    {
        node = GetParent<N>();
    }

    public void Start()
    {
        currentState = InitialState;
        States[currentState].Enter(node);
    }

    public override void _PhysicsProcess(double delta)
    {
        T newState = States[currentState].Update(node, delta);

        if (!newState.Equals(NoneState))
        {
            ChangeState(newState);
        }
    }

    public void ChangeState(T newState)
    {
        GD.Print(
            node.Name
                + " entering state "
                + newState.ToString()
                + " from state "
                + currentState.ToString()
        );

        var toState = States[newState];
        var fromState = States[currentState];

        // If we're entering a SubState we don't want to 'Exit' the current state
        if (toState is SubState<T, N> toSubstate)
        {
            toSubstate.PreviousState = currentState;
        }
        else
        {
            fromState.Exit(node);
        }

        currentState = newState;

        // If we're returning from a SubState we don't want to 'Enter' the new state, as we never exited it.
        if (!(fromState is SubState<T, N> fromSubstate))
        {
            toState.Enter(node);
        } 
    }
}
