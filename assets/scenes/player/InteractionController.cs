using System;
using System.Collections.Generic;
using Godot;

public partial class InteractionController : RayCast3D
{
    [Signal]
    public delegate void OnRaycastEnterEventHandler(Hoverable hoverable);

    [Signal]
    public delegate void OnRaycastExitEventHandler();

    [Signal]
    public delegate void OnBeginCarryEventHandler(PhysicsBody3D collider);

    [Signal]
    public delegate void OnEndCarryEventHandler();

    const float throwForce = 20.0f;
    const float dropForce = 0.0f;
    const float carryDropDistance = 3.0f;

    bool wasColliding = false;
    string wasCollidingWith = "";

    public Hoverable currentlyHovering;

    bool isCarrying = false;
    public bool IsCarrying
    {
        get => isCarrying;
    }

    bool pauseCarry = false;
    public bool PauseCarry
    {
        get => pauseCarry;
        set => pauseCarry = value;
    }

    Carryable carrying;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    public Interactable TryInteract()
    {
        PhysicsBody3D collider = (PhysicsBody3D)GetCollider();
        if (collider != null)
        {
            if (collider is Interactable interactable && interactable.CanInteract)
            {
                interactable.Interact();
                EmitSignal(SignalName.OnRaycastEnter, interactable);
                return interactable;
            } 
        }

        return null;
    }

    public override void _Process(double delta) { }

    public override void _PhysicsProcess(double delta)
    {
        PhysicsBody3D collider = (PhysicsBody3D)GetCollider();
        if (collider != null)
        {
            string colliderRID = collider.GetRid().ToString();

            if (wasColliding == false || wasCollidingWith != colliderRID)
            {
                // We're colliding with a new object!
                if (collider is Hoverable hoverable)
                {
                    if (wasColliding) {
                        EmitSignal(SignalName.OnRaycastExit);
                    }

                    if (!hoverable.HoverEnabled)
                        return;

                    EmitSignal(SignalName.OnRaycastEnter, hoverable);
                
                    wasColliding = true;
                    wasCollidingWith = colliderRID;
                    currentlyHovering = hoverable;
                }
                else
                {
                    ResetState();
                    EmitSignal(SignalName.OnRaycastExit);
                }
            }
            else if (wasColliding && wasCollidingWith == colliderRID)
            {
                // We're colliding with the same object!
                if (collider is Hoverable hoverable && !hoverable.HoverEnabled)
                {
                    ResetState();
                    EmitSignal(SignalName.OnRaycastExit);
                }
            }
        }
        else if (wasColliding)
        {
            ResetState();
            EmitSignal(SignalName.OnRaycastExit);
        }
    }

    public void ResetState()
    {
        wasColliding = false;
        wasCollidingWith = "";
    }

}
