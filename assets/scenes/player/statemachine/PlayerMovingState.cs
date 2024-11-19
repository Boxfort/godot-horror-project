using System;
using System.Collections.Generic;
using Godot;

internal class PlayerMovingState : State<PlayerState, PlayerController>
{
    public override PlayerState StateType => PlayerState.Moving;

    public override void Enter(PlayerController node)
    {
        node.canMoveHead = true;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void Exit(PlayerController node)
    {
        // no-op
    }

    public override PlayerState Update(PlayerController node, double delta)
    {
        node.HandleMovement(delta);
        node.HandleZoom(delta);

        if (Input.IsActionJustPressed("fire"))
        {
           Interactable interactable = node.interactionController.TryInteract();

            if (interactable != null)
            {
                if (interactable is ComputerController)
                {
                    node.interactingWith = interactable;
                    return PlayerState.UsingComputer;
                }
                else if (interactable is PhoneController)
                {
                    node.interactingWith = interactable;
                    return PlayerState.UsingPhone;
                } 
                else if (interactable is Stool) 
                {
                    return PlayerState.Sitting;
                }
            }
        }

        return PlayerState.None;
    }
}
