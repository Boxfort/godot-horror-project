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
            var stool= (Stool)node.GetTree().GetFirstNodeInGroup("stool");
            stool.PlayStandAudio();
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
                // We're now close enough to the chair, begin the sitting down portion
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

            var normalised = 1 - (distanceToFinal / distanceToSlidePoint);
            var inputPi = normalised * Mathf.Pi;

            var a = Mathf.Sin(inputPi) / 8;
            playerSitAnimPos.Y = a;

            // Slow down slightly and speed up as we sit
            playerTrackPosition = playerTrackPosition.MoveToward(
                playerTrackTarget,
                (float)delta * (0.8f + normalised)
            );
        } else {
            playerTrackPosition = playerTrackPosition.MoveToward(
                playerTrackTarget,
                (float)delta * (1 + playerTrackPosition.DistanceTo(playerTrackTarget))
            );
        }

        node.GlobalPosition = playerTrackPosition + playerSitAnimPos;

        if (node.GlobalPosition.IsEqualApprox(playerTrackTarget))
        {
            GD.Print("yoo");
            playerSitAnimPos = Vector3.Zero;
            entering = false;
            node.canMoveHead = true;
            slidingUp = false;
            return true;
        }

        return false;
    }

    public Vector3 stopInteractOffset => new Vector3(0.5f, 0, 0);

    private bool HandleExiting(PlayerController node, double delta)
    {
        var stool = (Stool)node.GetTree().GetFirstNodeInGroup("stool");
        var target = stool.GlobalPosition + stopInteractOffset;
        target.Y = playerHeight;

        Vector3 playerSitAnimPos = Vector3.Zero;
        float distanceToFinal = playerTrackPosition.DistanceTo(target);
        float totalDistanceToFinal = (new Vector3(stool.GlobalPosition.X, playerHeight, stool.GlobalPosition.Z)+sittingOffset).DistanceTo(target);

        var normalised = 1 - (distanceToFinal / totalDistanceToFinal);
        var inputPi = normalised * Mathf.Pi;

        var a = Mathf.Sin(inputPi) / 8;
        playerSitAnimPos.Y = a;

        // Slow down slightly and speed up as we sit
        playerTrackPosition = playerTrackPosition.MoveToward(
            target,
            (float)delta * (0.8f + normalised)
        );

        node.GlobalPosition = playerTrackPosition + playerSitAnimPos;

        if (node.GlobalPosition == target)
        {
            exiting = false;
            stool.CanInteract = true;
            stool.HoverEnabled = true;
            return true;
        }
        node.zoomView = false;

        return false;
    }
}
