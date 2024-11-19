using System;
using System.Collections.Generic;
using Godot;

internal class PlayerUsingPhoneState : SubState<PlayerState, PlayerController>
{
    public override PlayerState StateType => PlayerState.UsingComputer;

    bool exiting = false;
    bool entering = false;
    bool phonePickedUp = false;

    RotaryPhoneHole heldPhoneKey;

    Vector3 initialPlayerPosition;

    public override void Enter(PlayerController node)
    {
        if (node.interactingWith != null)
        {
            node.interactingWith.HoverEnabled = false;
        }

        initialPlayerPosition = node.GlobalPosition;
        node.canMoveHead = false;
        entering = true;
        exiting = false;
        phonePickedUp = false;
        node.handset.Show();
    }

    public override void Exit(PlayerController node)
    {
        if (node.interactingWith != null)
        {
            node.interactingWith.HoverEnabled = true;
        }

        node.interactingWith = null;
        exiting = false;
        phonePickedUp = false;
    }

    public override PlayerState Update(PlayerController node, double delta)
    {
        // Pre-conditions
        if (node.interactingWith == null)
        {
            GD.PrintErr("`interactingWith` was not set before entering `PlayerUsingPhoneState`.");
            return PlayerState.Sitting;
        }
        else if (!(node.interactingWith is PhoneController))
        {
            GD.PrintErr("`interactingWith` is not of type PhoneController.");
            return PlayerState.Sitting;
        }

        PhoneController phone = node.interactingWith as PhoneController;

        // Exiting
        if (!exiting && !entering && Input.IsActionJustPressed("fire2"))
        {
            node.RemoveInteractionException(phone);
            exiting = true;
            phone.HangupPhone();
            node.handset.Hide();
        }

        if (exiting)
        {
            bool hasExited = HandleExiting(node, delta);

            if (hasExited) {
                return PlayerState.Sitting;
            }
        }

        // Entering
        if (!phonePickedUp)
        {
            node.AddInteractionException(phone);
            phone.PickupPhone();
            phonePickedUp = true;
        }
        if (entering)
        {
            HandleEntering(node, delta);
        }

        node.HandleZoom(delta);

        if (Input.IsActionJustPressed("fire"))
        {
            Interactable interactable = node.interactionController.TryInteract();

            if (interactable is RotaryPhoneHole key) 
            {
                heldPhoneKey = key;
            }
        }

        if (heldPhoneKey != null) {
            phone.IncomingRay(node.GetHeadPosition(), node.GetLookDirection());
        }

        if (!Input.IsActionPressed("fire") && heldPhoneKey != null) {
            heldPhoneKey.StopInteract();
            heldPhoneKey = null;
        }

        return PlayerState.None;
    }

    public Vector3 interactOffset => new Vector3(5f, 0.5f, 0);
    public Vector3 stopInteractOffset => new Vector3(2.0f, 0.5f, 0);


    private void HandleEntering(PlayerController node, double delta)
    {
        var target = node.interactingWith.ToGlobal(interactOffset);

        node.GlobalPosition = node.GlobalPosition.MoveToward(
            target,
            (float)delta * (1 + node.GlobalPosition.DistanceTo(target))
        );
        node.LookAtSmooth(node.interactingWith.GlobalPosition, 10.0f, delta);

        if (node.GlobalPosition == target)
        {
            entering = false;
            node.canMoveHead = true;
        }
    }

    private bool HandleExiting(PlayerController node, double delta)
    {
        var interactPos = node.interactingWith.ToGlobal(interactOffset);
        var totalDistance = interactPos.DistanceTo(initialPlayerPosition);

        var currentDistance = node.GlobalPosition.DistanceTo(initialPlayerPosition);

        var normalised = 1 - (currentDistance / totalDistance);

        node.LookAtSmooth(node.interactingWith.GlobalPosition+(Vector3.Up*0.1f), 10.0f, delta);

        node.GlobalPosition = node.GlobalPosition.MoveToward(
            initialPlayerPosition,
            (float)delta * (1 + (normalised*3))
        );

        if (node.GlobalPosition.IsEqualApprox(initialPlayerPosition))
        {
            exiting = false;
            return true;
        }

        return false;
    }
}
