using Godot;
using System;

public partial class RotaryPhoneHole : Interactable
{
    [Signal]
    public delegate void OnBeginInteractionEventHandler();

    [Signal]
    public delegate void OnEndInteractionEventHandler();

    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    public override string InteractString => "Turn";

    [Export]
    public string hoverString = "0";
    public override string HoverString => hoverString;

    public override void Interact()
    {
        GD.Print("Begin interact with phone button " + hoverString);
        EmitSignal(SignalName.OnBeginInteraction);
    }

    public void StopInteract()
    {
        GD.Print("stopped interact with phone button " + hoverString);
        EmitSignal(SignalName.OnEndInteraction);
    }
}
