using Godot;
using System;

public partial class Stool : Interactable
{
    public override string InteractString => "Sit";

    public override string HoverString => "Stool";

    AudioStreamPlayer3D sitAudio1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sitAudio1 = GetNode<AudioStreamPlayer3D>("ChairSitAudio1");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void Interact()
    {
        // no-op
    }

    public void PlaySitAudio()
    {
        sitAudio1.Play();
    }

    public void PlayStandAudio()
    {

    }
}
