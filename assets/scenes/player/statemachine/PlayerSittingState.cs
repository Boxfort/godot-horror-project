using System;
using System.Collections.Generic;
using System.Runtime;
using Godot;

internal class PlayerSittingState : State<PlayerState, PlayerController>
{
    public override PlayerState StateType => PlayerState.UsingComputer;

    public PlayerState PreviousState
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    bool exiting = false;
    bool entering = false;
    Vector3 playerTrackPosition = Vector3.Zero;
    float playerHeight = 0;

    public override void Enter(PlayerController node)
    {
        if (node.interactingWith != null)
        {
            node.interactingWith.HoverEnabled = false;
        }

        playerTrackPosition = node.GlobalPosition;
        playerHeight = node.GlobalPosition.Y;
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
            // Just sit down
        }
        else if (!(node.interactingWith is ComputerController))
        {
            // after sitting go to computer state, todo same for phone
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
                return PlayerState.Moving;
            }
        }

        // Entering
        if (entering)
        {
            bool hasEntered = HandleEntering(node, delta);

            if (hasEntered)
            {
                // n/a
            }
        }

        node.HandleZoom(delta);

        if (!exiting && !entering && Input.IsActionJustPressed("fire"))
        {
            Interactable interactedWith = node.interactionController.TryInteract();

            if (interactedWith != null)
            {
                /*
                // TODO: ALLOW INTERACTING WITH PHONE AND PC
                if (interactedWith is Inspectable inspectable)
                {
                    node.CurrentlyInspecting = inspectable;
                    return PlayerState.Inspecting;
                }
                */
            }
        }

        return PlayerState.None;
    }

    public Vector3 sittingOffset => new Vector3(0.0f, -.0f, 0.0f);

    bool slidingUp = false;
    float distanceToSlidePoint = 0;

    private bool HandleEntering(PlayerController node, double delta)
    {
        var stool = (Stool)node.GetTree().GetFirstNodeInGroup("stool");
        stool.CanInteract = false;
        stool.HoverEnabled = false;

        Vector3 playerXYTarget = new Vector3(stool.GlobalPosition.X, playerHeight, stool.GlobalPosition.Z);
        Vector3 playerXY = new Vector3(node.GlobalPosition.X, playerHeight, node.GlobalPosition.Z);

        Vector3 playerSitAnimPos = Vector3.Zero;
        Vector3 playerTrackTarget = Vector3.Zero;

        if (!slidingUp) {
            float distanceToXY = playerXY.DistanceTo(playerXYTarget);
            if(distanceToXY < 0.5) {
                slidingUp = true;
                playerTrackTarget = playerXYTarget + sittingOffset;
                distanceToSlidePoint = playerTrackPosition.DistanceTo(playerTrackTarget);
                stool.PlaySitAudio();
            } else {
                playerTrackTarget = playerXYTarget;
            }
        } 
        
        if (slidingUp) {
            playerTrackTarget = playerXYTarget + sittingOffset;
            float distanceToFinal = playerTrackPosition.DistanceTo(playerTrackTarget);
            var input = (1 - (distanceToFinal / distanceToSlidePoint)) * Mathf.Pi;
            var a = Mathf.Sin(input) / 8;
            playerSitAnimPos.Y = a;
        }

        playerTrackPosition = playerTrackPosition.MoveToward(
            playerTrackTarget,
            (float)delta * (1 + playerTrackPosition.DistanceTo(playerTrackTarget))
        );

        if (node.GlobalPosition.IsEqualApprox(playerTrackTarget))
        {
            GD.Print("yoo");
            playerSitAnimPos = Vector3.Zero;
            entering = false;
            node.canMoveHead = true;
            slidingUp = false;
            return true;
        }

        node.GlobalPosition = playerTrackPosition + playerSitAnimPos;

        return false;
    }

    public Vector3 stopInteractOffset => new Vector3(0.5f, 0, 0);

    private bool HandleExiting(PlayerController node, double delta)
    {
        var stool = (Stool)node.GetTree().GetFirstNodeInGroup("stool");
        var target = stool.GlobalPosition + stopInteractOffset;
        target.Y = playerHeight;

        node.GlobalPosition = node.GlobalPosition.MoveToward(
            target,
            (float)delta * (1 + node.GlobalPosition.DistanceTo(target))
        );

        if (node.GlobalPosition == target)
        {
            exiting = false;
            node.canMoveHead = true;
            stool.CanInteract = true;
            stool.HoverEnabled = true;
            return true;
        }
        node.zoomView = false;

        return false;
    }
}
