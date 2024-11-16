using System;
using System.Collections.Generic;
using Godot;

public abstract partial class Inspectable : Carryable
{
    [Signal]
    public delegate void OnBeginInspectingEventHandler();

    [Signal]
    public delegate void OnEndInspectingEventHandler();

    Vector3 lastPosition;
    Vector3 lastRotationDegrees;
    protected bool isInspecting = false;

    public abstract Vector3 InspectOffset { get; }

    public abstract String ItemId { get; }

    public override void _Ready()
    {
        base._Ready();
    }

    protected virtual void SetCollisionFlags(bool shouldCollide)
    {
        SetCollisionMaskValue(2, shouldCollide);
        SetCollisionLayerValue(3, shouldCollide);
    }

    public virtual void OnInspect(PlayerController player)
    {
        EmitSignal(SignalName.OnBeginInspecting);
        lastPosition = GlobalPosition;
        lastRotationDegrees = GlobalRotationDegrees;

        isFalling = false;
        isInspecting = true;

        GlobalPosition =
            player.GetHeadPosition()
            - (
                (player.GetLookDirection() * InspectOffset.Z)
                - (player.GetHeadUpDirection() * InspectOffset.Y)
            );
        LookAt(player.GetHeadPosition() - (player.GetLookDirection() * 10));

        SetCollisionFlags(false);
    }

    public virtual void OnStopInspect()
    {
        EmitSignal(SignalName.OnEndInspecting);
        isInspecting = false;

        GlobalPosition = lastPosition;
        GlobalRotationDegrees = lastRotationDegrees;

        SetCollisionFlags(true);
    }
}
