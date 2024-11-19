using Godot;
using System;

public partial class Stool : Interactable
{
    public override string InteractString => "Sit";

    public override string HoverString => "Stool";

    AudioStreamPlayer3D sitAudio;
    AudioStreamPlayer3D standAudio;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sitAudio = GetNode<AudioStreamPlayer3D>("ChairSitAudio");
        standAudio = GetNode<AudioStreamPlayer3D>("ChairStandAudio");
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
        sitAudio.Play();
    }

    public void PlayStandAudio()
    {
        standAudio.Play();
    }
}
