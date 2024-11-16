using System;
using System.Collections.Generic;
using Godot;

internal class PlayerUsingComputerState : State<PlayerState, PlayerController>
{
    public override PlayerState StateType => PlayerState.UsingComputer;

    public PlayerState PreviousState
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    bool exiting = false;
    bool entering = false;

    public override void Enter(PlayerController node)
    {
        if (node.interactingWith!= null)
        {
            node.interactingWith.HoverEnabled = false;
        }

        node.canMoveHead = false;
        entering = true;
        exiting = false;
    }

    public override void Exit(PlayerController node)
    {
        if (node.interactingWith != null)
        {
            node.interactingWith.HoverEnabled = true;
        }

        node.interactingWith = null;
        exiting = false;
    }

    public override PlayerState Update(PlayerController node, double delta)
    {
        // Pre-conditions
        if (node.interactingWith == null)
        {
            GD.PrintErr(
                "`interactingWith` was not set before entering `PlayerUsingComputerState`."
            );
            return PlayerState.Moving;
        }
        else if (!(node.interactingWith is ComputerController))
        {
            GD.PrintErr("`interactingWith` is not of type ComputerController.");
            return PlayerState.Moving;
        }

        ComputerController computer = node.interactingWith as ComputerController;

        // Exiting
        if (!exiting && !entering && Input.IsActionJustPressed("fire2"))
        {
            exiting = true;
        }
        if (exiting)
        {
            bool hasExited = HandleExiting(node, delta);

            if (hasExited)
            {
                computer.OnEndInteracting();
                return PlayerState.Moving;
            }
        }

        // Entering
        if (entering)
        {
            bool hasEntered = HandleEntering(node, delta);

            if (hasEntered)
            {
                computer.OnBeginInteracting();
            }
        }

        node.HandleZoom(delta);

        if (Input.IsActionJustPressed("fire"))
        {
            Interactable interactedWith = node.interactionController.TryInteract();

            if (interactedWith != null)
            {
                if (interactedWith is Inspectable inspectable)
                {
                    node.CurrentlyInspecting = inspectable;
                    return PlayerState.Inspecting;
                }
            }
        }

        computer.IncomingRay(node.GetHeadPosition(), node.GetLookDirection());

        return PlayerState.None;
    }

    public Vector3 interactOffset => new Vector3(0.80f, -0.70f, 0);

    private bool HandleEntering(PlayerController node, double delta)
    {
        var target = node.interactingWith.ToGlobal(interactOffset);

        ComputerController computer = node.interactingWith as ComputerController;
        computer.IncomingRay(node.GetHeadPosition(), node.GetLookDirection());
        computer.CanInteract = false;
        computer.HoverEnabled = false;

        node.GlobalPosition = node.GlobalPosition.MoveToward(
            target,
            (float)delta * (1 + node.GlobalPosition.DistanceTo(target))
        );
        node.LookAtSmooth(node.interactingWith.GlobalPosition, 10.0f, delta);

        if (node.GlobalPosition == target)
        {
            entering = false;
            node.canMoveHead = true;
            return true;
        }

        //node.zoomView = false;

        return false;
    }

    public Vector3 stopInteractOffset => new Vector3(1.0f, -0.56f, 0);

    private bool HandleExiting(PlayerController node, double delta)
    {
        var target = node.interactingWith.ToGlobal(stopInteractOffset);
        ((ComputerController)node.interactingWith).DisableScreenGuiInput(true);

        node.GlobalPosition = node.GlobalPosition.MoveToward(
            target,
            (float)delta * (1 + node.GlobalPosition.DistanceTo(target))
        );

        if (node.GlobalPosition == target)
        {
            entering = false;
            node.canMoveHead = true;
            node.zoomView = false;
            ComputerController computer = node.interactingWith as ComputerController;
            computer.CanInteract = true;
            computer.HoverEnabled = true;
            return true;
        }
        node.zoomView = false;

        return false;
    }
}
